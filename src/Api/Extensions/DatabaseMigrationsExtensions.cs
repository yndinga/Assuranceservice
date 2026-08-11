using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace AssuranceService.Api.Extensions;

/// <summary>
/// Au dÃ©marrage : crÃ©e la base de donnÃ©es si elle n'existe pas, puis crÃ©e/met Ã  jour toutes les tables (migrations EF).
/// DÃ©sactiver avec ApplyMigrationsAtStartup=false dans la configuration.
/// </summary>
public static class DatabaseMigrationsExtensions
{
    private const int MaxRetries = 5;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

    public static async Task ApplyMigrationsAtStartupAsync(this WebApplication app, CancellationToken cancellationToken = default)
    {
        var config = app.Configuration;
        if (string.Equals(config["ApplyMigrationsAtStartup"], "false", StringComparison.OrdinalIgnoreCase))
        {
            app.Logger.LogInformation("[DÃ©marrage] ApplyMigrationsAtStartup=false : migrations dÃ©sactivÃ©es, vÃ©rification minimale de la table [Assurances].");
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AssuranceService.Infrastructure.Data.AssuranceDbContext>();
                if (!await AssurancesTableExistsAsync(context, cancellationToken))
            {
                await EnsureAssurancesTableAsync(context, cancellationToken);
                await EnsureCurrentAssuranceSchemaAsync(context, cancellationToken);
                await EnsureTransportTablesAsync(context, cancellationToken);
                app.Logger.LogInformation("[DÃ©marrage] Table [Assurances] crÃ©Ã©e (mode minimal).");
            }
            return;
        }

        var connectionString = NormalizeSqlServerConnectionString(
            config.GetConnectionString("AssuranceConnection")
            ?? "Server=localhost,1420;Database=MS_ASSURANCE;User Id=sa;Password=DevStrongPwd@123;TrustServerCertificate=True;");
        var serverHint = MaskConnectionStringForLog(connectionString);
        app.Logger.LogInformation("[DÃ©marrage] CrÃ©ation de la base et des tables si nÃ©cessaire. Connexion : {ServerHint}", serverHint);

        var logger = app.Logger;
        var contextType = typeof(AssuranceService.Infrastructure.Data.AssuranceDbContext);

        for (var attempt = 1; attempt <= MaxRetries; attempt++)
        {
            try
            {
                // 1. CrÃ©er la base sur master (mÃªme boucle de tentatives que les migrations)
                await EnsureDatabaseExistsAsync(connectionString, logger, cancellationToken);

                // 2. CrÃ©er ou mettre Ã  jour les tables (migrations EF, ou EnsureCreated si besoin)
                using var scope = app.Services.CreateScope();
                var services = scope.ServiceProvider;
                var context = (DbContext)services.GetRequiredService(contextType);
                var pending = await context.Database.GetPendingMigrationsAsync(cancellationToken);
                var pendingList = pending.ToList();

                logger.LogInformation("[DÃ©marrage] Migrations en attente : {Count} ({Migrations}).",
                    pendingList.Count, pendingList.Count > 0 ? string.Join(", ", pendingList) : "aucune");

                if (pendingList.Count > 0)
                {
                    // EF ne dÃ©tecte parfois que les migrations ayant un .Designer.cs : la 1Ã¨re migration appliquÃ©e
                    // peut alors Ãªtre une migration Â« incrÃ©mentale Â» alors que la base est vide (pas de table Primes, etc.).
                    if (!await AssurancesTableExistsAsync(context, cancellationToken))
                    {
                        logger.LogWarning(
                            "[DÃ©marrage] Table mÃ©tier [Assurances] absente alors que {Count} migration(s) sont en attente. " +
                            "CrÃ©ation du schÃ©ma depuis le modÃ¨le actuel (EnsureCreated) puis enregistrement de toutes les migrations connues d'EF dans __EFMigrationsHistory.",
                            pendingList.Count);
                        await EnsureAssurancesTableAsync(context, cancellationToken);
                        await EnsureTransportTablesAsync(context, cancellationToken);
                        await EnsureMigrationsHistoryTableAsync(context, cancellationToken);
                        await MarkAllEfMigrationsAsAppliedAsync(context, logger, cancellationToken);
                        logger.LogInformation("[DÃ©marrage] SchÃ©ma et historique des migrations synchronisÃ©s avec le modÃ¨le EF.");
                        return;
                    }

                    await context.Database.MigrateAsync(cancellationToken);
                    await EnsureCurrentAssuranceSchemaAsync(context, cancellationToken);
                    await EnsureCurrentTransportSchemaAsync(context, cancellationToken);
                    logger.LogInformation("[DÃ©marrage] Migrations appliquÃ©es. Base et tables Ã  jour.");
                    return;
                }

                // Fallback : si EF ne dÃ©tecte aucune migration, crÃ©er les tables au dÃ©marrage avec EnsureCreated()
                // sauf si le schÃ©ma est dÃ©jÃ  en place (historique prÃ©sent ET au moins une table mÃ©tier).
                var canConnect = await context.Database.CanConnectAsync(cancellationToken);
                if (!canConnect)
                {
                    logger.LogWarning("[DÃ©marrage] Impossible de se connecter Ã  la base.");
                    throw new InvalidOperationException("Impossible de se connecter Ã  la base.");
                }

                var historyExists = await HistoryTableExistsAsync(context, cancellationToken);
                if (!historyExists)
                {
                    logger.LogInformation("[DÃ©marrage] Aucune table d'historique : crÃ©ation des tables au dÃ©marrage (EnsureCreated).");
                    await EnsureAssurancesTableAsync(context, cancellationToken);
                    await EnsureCurrentAssuranceSchemaAsync(context, cancellationToken);
                    await EnsureTransportTablesAsync(context, cancellationToken);
                    await EnsureMigrationsHistoryTableAsync(context, cancellationToken);
                    await MarkAllEfMigrationsAsAppliedAsync(context, logger, cancellationToken);
                    logger.LogInformation("[DÃ©marrage] Tables crÃ©Ã©es et toutes les migrations EF enregistrÃ©es dans l'historique.");
                    return;
                }

                // Auto-rÃ©paration : historique EF prÃ©sent mais table mÃ©tier absente (ex: suppression manuelle)
                if (!await AssurancesTableExistsAsync(context, cancellationToken))
                {
                    logger.LogWarning(
                        "[DÃ©marrage] Historique EF prÃ©sent mais table [Assurances] absente. " +
                        "RecrÃ©ation du schÃ©ma via EnsureCreated puis resynchronisation de l'historique.");
                    await context.Database.EnsureCreatedAsync(cancellationToken);
                    await EnsureMigrationsHistoryTableAsync(context, cancellationToken);
                    await MarkAllEfMigrationsAsAppliedAsync(context, logger, cancellationToken);
                    logger.LogInformation("[DÃ©marrage] SchÃ©ma recrÃ©Ã© et historique EF resynchronisÃ©.");
                    return;
                }

                await EnsureCurrentAssuranceSchemaAsync(context, cancellationToken);
                await EnsureCurrentTransportSchemaAsync(context, cancellationToken);
                logger.LogInformation("[DÃ©marrage] Base et tables dÃ©jÃ  Ã  jour (aucune migration en attente).");
                return;
            }
            catch (Exception ex)
            {
                LogSqlConnectionTroubleshooting(logger, ex);
                logger.LogWarning(ex,
                    "[DÃ©marrage] Tentative {Attempt}/{Max} Ã©chouÃ©e. RÃ©essai dans {Delay}s. Connexion : {ServerHint}",
                    attempt, MaxRetries, RetryDelay.TotalSeconds, serverHint);

                if (attempt == MaxRetries)
                {
                    logger.LogError(
                        "[DÃ©marrage] Ã‰chec aprÃ¨s {Max} tentatives. Pour dÃ©marrer sans crÃ©er la base/tables : ApplyMigrationsAtStartup=false. Connexion : {ServerHint}",
                        MaxRetries, serverHint);
                    logger.LogError(ex, "[DÃ©marrage] DÃ©tail de l'exception.");
                    throw;
                }

                await Task.Delay(RetryDelay, cancellationToken);
            }
        }
    }

    /// <summary>
    /// Se connecte Ã  master et crÃ©e la base si elle n'existe pas (Ã©vite erreur 4060 "Cannot open database").
    /// </summary>
    private static async Task EnsureDatabaseExistsAsync(string connectionString, ILogger logger, CancellationToken cancellationToken)
    {
        try
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            var databaseName = builder.InitialCatalog;
            if (string.IsNullOrWhiteSpace(databaseName))
            {
                logger.LogWarning("[DÃ©marrage] Aucune base (Initial Catalog) dans la chaÃ®ne de connexion.");
                return;
            }

            builder.InitialCatalog = "master";
            // Ne pas assigner null Ã  AttachDBFilename (ArgumentNullException) ; utiliser Empty pour ne pas attacher de fichier
            builder.AttachDBFilename = string.Empty;
            using var connection = new SqlConnection(builder.ConnectionString);
            await connection.OpenAsync(cancellationToken);

            var cmd = connection.CreateCommand();
            cmd.CommandText = $"""
                IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = N'{databaseName.Replace("'", "''")}')
                BEGIN
                    CREATE DATABASE [{databaseName.Replace("]", "]]")}];
                END
                """;
            await cmd.ExecuteNonQueryAsync(cancellationToken);
            logger.LogInformation("[DÃ©marrage] Base {Database} vÃ©rifiÃ©e ou crÃ©Ã©e.", databaseName);
        }
        catch (Exception ex)
        {
            LogSqlConnectionTroubleshooting(logger, ex);
            logger.LogWarning(ex, "[DÃ©marrage] Impossible de crÃ©er ou d'atteindre la base sur le serveur SQL (voir message dÃ©taillÃ© ci-dessus).");
            throw;
        }
    }

    /// <summary>
    /// Corrige des erreurs frÃ©quentes (ex. espace aprÃ¨s la virgule du port : "localhost, 1420").
    /// </summary>
    private static string NormalizeSqlServerConnectionString(string connectionString)
    {
        try
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            if (!string.IsNullOrEmpty(builder.DataSource))
            {
                // "host, 1433" â†’ "host,1433"
                var ds = builder.DataSource.Trim();
                while (ds.Contains(", ", StringComparison.Ordinal))
                    ds = ds.Replace(", ", ",", StringComparison.Ordinal);
                builder.DataSource = ds;
            }

            return builder.ConnectionString;
        }
        catch
        {
            return connectionString;
        }
    }

    private static void LogSqlConnectionTroubleshooting(ILogger logger, Exception ex)
    {
        if (ex is not SqlException sql)
        {
            return;
        }

        // 10061 : connexion refusÃ©e ; -2 : timeout
        if (sql.Number is 10061 or 11001 or -2)
        {
            logger.LogWarning(
                "[DÃ©marrage] SQL Server inaccessible (erreur {Number}). ContrÃ´lez : le service SQL est dÃ©marrÃ© ; " +
                "le port TCP dans ConnectionStrings correspond Ã  celui configurÃ© (souvent 1433 pour lâ€™instance par dÃ©faut) ; " +
                "TCP/IP est activÃ© dans le Gestionnaire de configuration SQL Server ; " +
                "pas dâ€™espace aprÃ¨s la virgule dans Server=machine,port.",
                sql.Number);
        }
    }

    /// <summary>
    /// VÃ©rifie si la table __EFMigrationsHistory existe (migrations dÃ©jÃ  utilisÃ©es).
    /// </summary>
    private static async Task<bool> HistoryTableExistsAsync(DbContext context, CancellationToken cancellationToken)
    {
        try
        {
            var conn = context.Database.GetDbConnection();
            await conn.OpenAsync(cancellationToken);
            try
            {
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = """
                    IF OBJECT_ID(N'[dbo].[__EFMigrationsHistory]', N'U') IS NOT NULL
                        SELECT 1 ELSE SELECT 0
                    """;
                var result = await cmd.ExecuteScalarAsync(cancellationToken);
                return result is int i && i == 1;
            }
            finally
            {
                await context.Database.CloseConnectionAsync();
            }
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// CrÃ©e la table __EFMigrationsHistory si elle n'existe pas (EnsureCreated ne la crÃ©e pas).
    /// </summary>
    private static async Task EnsureMigrationsHistoryTableAsync(DbContext context, CancellationToken cancellationToken)
    {
        await context.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'[dbo].[__EFMigrationsHistory]', N'U') IS NULL
            CREATE TABLE [__EFMigrationsHistory] (
                [MigrationId] nvarchar(150) NOT NULL,
                [ProductVersion] nvarchar(32) NOT NULL,
                CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
            );
            """, cancellationToken);
    }

    /// <summary>
    /// Indique si la table Assurances existe (repÃ¨re qu'un schÃ©ma mÃ©tier a dÃ©jÃ  Ã©tÃ© crÃ©Ã© par migrations initiales).
    /// </summary>
    private static async Task<bool> AssurancesTableExistsAsync(DbContext context, CancellationToken cancellationToken)
    {
        try
        {
            var conn = context.Database.GetDbConnection();
            await conn.OpenAsync(cancellationToken);
            try
            {
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = """
                    IF OBJECT_ID(N'[dbo].[Assurances]', N'U') IS NOT NULL
                        SELECT 1 ELSE SELECT 0
                    """;
                var result = await cmd.ExecuteScalarAsync(cancellationToken);
                return result is int i && i == 1;
            }
            finally
            {
                await context.Database.CloseConnectionAsync();
            }
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// CrÃ©e la table Assurances si absente (cas base partiellement initialisÃ©e).
    /// </summary>
    private static async Task EnsureAssurancesTableAsync(DbContext context, CancellationToken cancellationToken)
    {
        await context.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'[dbo].[Assurances]', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[Assurances](
                    [Id] uniqueidentifier NOT NULL,
                    [NumeroAFI] nvarchar(50) NOT NULL,
                    [NoPolice] nvarchar(255) NULL,
                    [NoFacture] nvarchar(250) NULL,
                    [NumeroCert] nvarchar(25) NULL,
                    [ImportateurNom] nvarchar(250) NOT NULL,
                    [ImportateurNIU] nvarchar(250) NOT NULL,
                    [DateDebut] datetime2 NULL,
                    [DateFin] datetime2 NULL,
                    [TypeContrat] nvarchar(250) NOT NULL,
                    [Duree] nvarchar(250) NULL,
                    [DureeJours] int NULL,
                    [Statut] nvarchar(10) NOT NULL CONSTRAINT [DF_Assurances_Statut] DEFAULT N'42',
                    [Module] nvarchar(250) NOT NULL,
                    [GarantieId] uniqueidentifier NULL,
                    [Partenaire] nvarchar(250) NULL,
                    [Intermediaire] nvarchar(250) NULL,
                    [TypePartenaire] nvarchar(50) NOT NULL CONSTRAINT [DF_Assurances_TypePartenaire] DEFAULT N'ASSUREUR',
                    [OCRE] nvarchar(250) NOT NULL,
                    [Designation] nvarchar(255) NULL,
                    [Nature] nvarchar(500) NULL,
                    [Specificites] nvarchar(100) NULL,
                    [Conditionnement] nvarchar(500) NULL,
                    [Description] nvarchar(500) NULL,
                    [Devise] nvarchar(50) NULL,
                    [MasseBrute] nvarchar(255) NULL,
                    [UniteStatistique] nvarchar(255) NULL,
                    [Marque] nvarchar(255) NULL,
                    [NomTransporteur] nvarchar(255) NULL,
                    [NomNavire] nvarchar(255) NULL,
                    [TypeNavire] nvarchar(100) NULL,
                    [LieuSejour] nvarchar(255) NULL,
                    [DureeSejour] nvarchar(50) NULL,
                    [PaysProvenance] nvarchar(255) NULL,
                    [PaysDestination] nvarchar(255) NULL,
                    [CreerPar] nvarchar(max) NULL,
                    [ModifierPar] nvarchar(max) NULL,
                    [CreerLe] datetime2 NOT NULL,
                    [ModifierLe] datetime2 NULL,
                    CONSTRAINT [PK_Assurances] PRIMARY KEY ([Id])
                );

                CREATE UNIQUE INDEX [IX_Assurances_NoPolice] ON [dbo].[Assurances]([NoPolice]) WHERE [NoPolice] IS NOT NULL;
                CREATE UNIQUE INDEX [IX_Assurances_NumeroCert] ON [dbo].[Assurances]([NumeroCert]) WHERE [NumeroCert] IS NOT NULL;
                CREATE UNIQUE INDEX [IX_Assurances_NumeroAFI] ON [dbo].[Assurances]([NumeroAFI]);

                IF OBJECT_ID(N'[dbo].[Garanties]', N'U') IS NOT NULL
                BEGIN
                    ALTER TABLE [dbo].[Assurances] WITH CHECK
                    ADD CONSTRAINT [FK_Assurances_Garanties_GarantieId]
                    FOREIGN KEY([GarantieId]) REFERENCES [dbo].[Garanties]([Id]);
                END
            END
            """, cancellationToken);
    }

    /// <summary>
    /// Rattrape les bases existantes dont l'historique EF est dÃ©jÃ  synchronisÃ© mais oÃ¹ la colonne mÃ©tier validÃ©e manque.
    /// Aligne les colonnes metier actuelles de l'assurance sans recreer d'anciens liens dossier/commande.
    /// </summary>
    private static async Task EnsureCurrentAssuranceSchemaAsync(DbContext context, CancellationToken cancellationToken)
    {
        await context.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'[dbo].[Assurances]', N'U') IS NOT NULL
            BEGIN
                IF COL_LENGTH(N'dbo.Assurances', N'NoFacture') IS NULL
                    ALTER TABLE [dbo].[Assurances] ADD [NoFacture] nvarchar(250) NULL;

                IF COL_LENGTH(N'dbo.Assurances', N'Partenaire') IS NULL
                    ALTER TABLE [dbo].[Assurances] ADD [Partenaire] nvarchar(250) NULL;

                IF COL_LENGTH(N'dbo.Assurances', N'Intermediaire') IS NULL
                    ALTER TABLE [dbo].[Assurances] ADD [Intermediaire] nvarchar(250) NULL;

                IF COL_LENGTH(N'dbo.Assurances', N'TypePartenaire') IS NULL
                    ALTER TABLE [dbo].[Assurances] ADD [TypePartenaire] nvarchar(50) NOT NULL CONSTRAINT [DF_Assurances_TypePartenaire] DEFAULT N'ASSUREUR';

                IF COL_LENGTH(N'dbo.Assurances', N'DureeJours') IS NULL
                    ALTER TABLE [dbo].[Assurances] ADD [DureeJours] int NULL;

                IF COL_LENGTH(N'dbo.Assurances', N'NumeroAFI') IS NULL
                    ALTER TABLE [dbo].[Assurances] ADD [NumeroAFI] nvarchar(50) NULL;

                UPDATE [dbo].[Assurances]
                SET [NumeroAFI] = UPPER(CONCAT(N'AFI-', REPLACE(CONVERT(nvarchar(36), [Id]), N'-', N'')))
                WHERE [NumeroAFI] IS NULL OR LTRIM(RTRIM([NumeroAFI])) = N'';

                UPDATE [dbo].[Assurances]
                SET [DureeJours] = TRY_CONVERT(int, NULLIF(LTRIM(RTRIM([Duree])), N''))
                WHERE [DureeJours] IS NULL
                  AND TRY_CONVERT(int, NULLIF(LTRIM(RTRIM([Duree])), N'')) > 0;

                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Assurances]') AND name = N'NumeroAFI' AND is_nullable = 1)
                    ALTER TABLE [dbo].[Assurances] ALTER COLUMN [NumeroAFI] nvarchar(50) NOT NULL;

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Assurances]') AND name = N'IX_Assurances_NumeroAFI')
                    CREATE UNIQUE INDEX [IX_Assurances_NumeroAFI] ON [dbo].[Assurances]([NumeroAFI]);
            END

            IF OBJECT_ID(N'[dbo].[Documents]', N'U') IS NOT NULL
            BEGIN
                IF COL_LENGTH(N'dbo.Documents', N'TypeDocument') IS NULL
                    ALTER TABLE [dbo].[Documents] ADD [TypeDocument] nvarchar(50) NOT NULL CONSTRAINT [DF_Documents_TypeDocument] DEFAULT N'PIECE_ASSURANCE';

                UPDATE [dbo].[Documents]
                SET [TypeDocument] = N'FACTURE_DI'
                WHERE [Description] LIKE N'Facture DI %';
            END

            IF OBJECT_ID(N'[dbo].[VisaAssurances]', N'U') IS NOT NULL
            BEGIN
                IF COL_LENGTH(N'dbo.VisaAssurances', N'DateVisa') IS NULL
                    ALTER TABLE [dbo].[VisaAssurances] ADD [DateVisa] datetime2(0) NULL;
                IF COL_LENGTH(N'dbo.VisaAssurances', N'Licence') IS NULL
                    ALTER TABLE [dbo].[VisaAssurances] ADD [Licence] nvarchar(250) NULL;
                IF COL_LENGTH(N'dbo.VisaAssurances', N'Message') IS NULL
                    ALTER TABLE [dbo].[VisaAssurances] ADD [Message] nvarchar(1000) NULL;

                IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[VisaAssurances]') AND name = N'IX_VisaAssurances_AssuranceId')
                    DROP INDEX [IX_VisaAssurances_AssuranceId] ON [dbo].[VisaAssurances];

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[VisaAssurances]') AND name = N'IX_VisaAssurances_AssuranceId_TypePartenaire_Organisation')
                    CREATE UNIQUE INDEX [IX_VisaAssurances_AssuranceId_TypePartenaire_Organisation]
                    ON [dbo].[VisaAssurances]([AssuranceId], [TypePartenaire], [Organisation]);

                UPDATE [dbo].[VisaAssurances]
                SET [DateVisa] = COALESCE([ModifierLe], [CreerLe])
                WHERE [VisaOK] = 1 AND [DateVisa] IS NULL;
            END
            """, cancellationToken);
    }

    /// <summary>
    /// Rattrape les bases locales creees avec l'ancien schema minimal alors que l'historique EF est deja applique.
    /// </summary>
    private static async Task EnsureCurrentTransportSchemaAsync(DbContext context, CancellationToken cancellationToken)
    {
        await context.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'[dbo].[Voyages]', N'U') IS NOT NULL
            BEGIN
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Voyages]') AND name = N'DescriptionConditionnement')
                    ALTER TABLE [dbo].[Voyages] ADD [DescriptionConditionnement] nvarchar(500) NULL;
            END

            IF OBJECT_ID(N'[dbo].[Maritimes]', N'U') IS NOT NULL
            BEGIN
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Maritimes]') AND name = N'VoyageId')
                    ALTER TABLE [dbo].[Maritimes] ADD [VoyageId] uniqueidentifier NULL;

                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Maritimes]') AND name = N'AssuranceId')
                   AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Maritimes]') AND name = N'VoyageId')
                   AND OBJECT_ID(N'[dbo].[Voyages]', N'U') IS NOT NULL
                    EXEC(N'UPDATE m
                           SET [VoyageId] = v.[Id]
                           FROM [dbo].[Maritimes] m
                           INNER JOIN [dbo].[Voyages] v ON v.[AssuranceId] = m.[AssuranceId]
                           WHERE m.[VoyageId] IS NULL;');

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Maritimes]') AND name = N'NumeroBL')
                    ALTER TABLE [dbo].[Maritimes] ADD [NumeroBL] nvarchar(255) NULL;

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Maritimes]') AND name = N'NomNavire')
                    ALTER TABLE [dbo].[Maritimes] ADD [NomNavire] nvarchar(255) NULL;

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Maritimes]') AND name = N'TypeNavire')
                    ALTER TABLE [dbo].[Maritimes] ADD [TypeNavire] nvarchar(100) NULL;

                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Maritimes]') AND name = N'PortEmbarquementId' AND is_nullable = 0)
                    ALTER TABLE [dbo].[Maritimes] ALTER COLUMN [PortEmbarquementId] uniqueidentifier NULL;

                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Maritimes]') AND name = N'PortDebarquementId' AND is_nullable = 0)
                    ALTER TABLE [dbo].[Maritimes] ALTER COLUMN [PortDebarquementId] uniqueidentifier NULL;

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Maritimes]') AND name = N'PortEmbarquementCode')
                    ALTER TABLE [dbo].[Maritimes] ADD [PortEmbarquementCode] nvarchar(50) NULL;

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Maritimes]') AND name = N'PortDebarquementCode')
                    ALTER TABLE [dbo].[Maritimes] ADD [PortDebarquementCode] nvarchar(50) NULL;
            END

            IF OBJECT_ID(N'[dbo].[Aeriens]', N'U') IS NOT NULL
            BEGIN
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Aeriens]') AND name = N'VoyageId')
                    ALTER TABLE [dbo].[Aeriens] ADD [VoyageId] uniqueidentifier NULL;

                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Aeriens]') AND name = N'AssuranceId')
                   AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Aeriens]') AND name = N'VoyageId')
                   AND OBJECT_ID(N'[dbo].[Voyages]', N'U') IS NOT NULL
                    EXEC(N'UPDATE a
                           SET [VoyageId] = v.[Id]
                           FROM [dbo].[Aeriens] a
                           INNER JOIN [dbo].[Voyages] v ON v.[AssuranceId] = a.[AssuranceId]
                           WHERE a.[VoyageId] IS NULL;');

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Aeriens]') AND name = N'NumeroLTA')
                    ALTER TABLE [dbo].[Aeriens] ADD [NumeroLTA] nvarchar(255) NULL;

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Aeriens]') AND name = N'AeroportEmbarquementCode')
                    ALTER TABLE [dbo].[Aeriens] ADD [AeroportEmbarquementCode] nvarchar(50) NULL;

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Aeriens]') AND name = N'AeroportDebarquementCode')
                    ALTER TABLE [dbo].[Aeriens] ADD [AeroportDebarquementCode] nvarchar(50) NULL;
            END

            IF OBJECT_ID(N'[dbo].[Routiers]', N'U') IS NOT NULL
            BEGIN
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Routiers]') AND name = N'VoyageId')
                    ALTER TABLE [dbo].[Routiers] ADD [VoyageId] uniqueidentifier NULL;

                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Routiers]') AND name = N'AssuranceId')
                   AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Routiers]') AND name = N'VoyageId')
                   AND OBJECT_ID(N'[dbo].[Voyages]', N'U') IS NOT NULL
                    EXEC(N'UPDATE r
                           SET [VoyageId] = v.[Id]
                           FROM [dbo].[Routiers] r
                           INNER JOIN [dbo].[Voyages] v ON v.[AssuranceId] = r.[AssuranceId]
                           WHERE r.[VoyageId] IS NULL;');

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Routiers]') AND name = N'NumeroLV')
                    ALTER TABLE [dbo].[Routiers] ADD [NumeroLV] nvarchar(255) NULL;

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Routiers]') AND name = N'RouteNationaleCode')
                    ALTER TABLE [dbo].[Routiers] ADD [RouteNationaleCode] nvarchar(50) NULL;
            END

            IF OBJECT_ID(N'[dbo].[Fluviaux]', N'U') IS NOT NULL
            BEGIN
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Fluviaux]') AND name = N'VoyageId')
                    ALTER TABLE [dbo].[Fluviaux] ADD [VoyageId] uniqueidentifier NULL;

                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Fluviaux]') AND name = N'AssuranceId')
                   AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Fluviaux]') AND name = N'VoyageId')
                   AND OBJECT_ID(N'[dbo].[Voyages]', N'U') IS NOT NULL
                    EXEC(N'UPDATE f
                           SET [VoyageId] = v.[Id]
                           FROM [dbo].[Fluviaux] f
                           INNER JOIN [dbo].[Voyages] v ON v.[AssuranceId] = f.[AssuranceId]
                           WHERE f.[VoyageId] IS NULL;');

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Fluviaux]') AND name = N'NomNavire')
                    ALTER TABLE [dbo].[Fluviaux] ADD [NomNavire] nvarchar(255) NULL;

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Fluviaux]') AND name = N'TypeNavire')
                    ALTER TABLE [dbo].[Fluviaux] ADD [TypeNavire] nvarchar(100) NULL;

                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Fluviaux]') AND name = N'PortEmbarquementId' AND is_nullable = 0)
                    ALTER TABLE [dbo].[Fluviaux] ALTER COLUMN [PortEmbarquementId] uniqueidentifier NULL;

                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Fluviaux]') AND name = N'PortDebarquementId' AND is_nullable = 0)
                    ALTER TABLE [dbo].[Fluviaux] ALTER COLUMN [PortDebarquementId] uniqueidentifier NULL;

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Fluviaux]') AND name = N'PortEmbarquementCode')
                    ALTER TABLE [dbo].[Fluviaux] ADD [PortEmbarquementCode] nvarchar(50) NULL;

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Fluviaux]') AND name = N'PortDebarquementCode')
                    ALTER TABLE [dbo].[Fluviaux] ADD [PortDebarquementCode] nvarchar(50) NULL;
            END
            """, cancellationToken);
    }

    /// <summary>
    /// CrÃ©e les tables de transport liÃ©es Ã  Assurance si absentes.
    /// </summary>
    private static async Task EnsureTransportTablesAsync(DbContext context, CancellationToken cancellationToken)
    {
        await context.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'[dbo].[Assurances]', N'U') IS NULL
                RETURN;

            IF OBJECT_ID(N'[dbo].[Voyages]', N'U') IS NULL
                RETURN;

            IF OBJECT_ID(N'[dbo].[Aeriens]', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[Aeriens](
                    [Id] uniqueidentifier NOT NULL,
                    [VoyageId] uniqueidentifier NOT NULL,
                    [AeroportEmbarquementCode] nvarchar(50) NULL,
                    [AeroportDebarquementCode] nvarchar(50) NULL,
                    [NumeroLTA] nvarchar(255) NULL,
                    [CreerPar] nvarchar(max) NULL,
                    [ModifierPar] nvarchar(max) NULL,
                    [CreerLe] datetime2 NOT NULL,
                    [ModifierLe] datetime2 NULL,
                    CONSTRAINT [PK_Aeriens] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_Aeriens_Voyages_VoyageId] FOREIGN KEY([VoyageId]) REFERENCES [dbo].[Voyages]([Id]) ON DELETE CASCADE
                );
                CREATE UNIQUE INDEX [IX_Aeriens_VoyageId] ON [dbo].[Aeriens]([VoyageId]);
            END

            IF OBJECT_ID(N'[dbo].[Maritimes]', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[Maritimes](
                    [Id] uniqueidentifier NOT NULL,
                    [VoyageId] uniqueidentifier NOT NULL,
                    [PortEmbarquementCode] nvarchar(50) NULL,
                    [PortDebarquementCode] nvarchar(50) NULL,
                    [NumeroBL] nvarchar(255) NULL,
                    [NomNavire] nvarchar(255) NULL,
                    [TypeNavire] nvarchar(100) NULL,
                    [CreerPar] nvarchar(max) NULL,
                    [ModifierPar] nvarchar(max) NULL,
                    [CreerLe] datetime2 NOT NULL,
                    [ModifierLe] datetime2 NULL,
                    CONSTRAINT [PK_Maritimes] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_Maritimes_Voyages_VoyageId] FOREIGN KEY([VoyageId]) REFERENCES [dbo].[Voyages]([Id]) ON DELETE CASCADE
                );
                CREATE UNIQUE INDEX [IX_Maritimes_VoyageId] ON [dbo].[Maritimes]([VoyageId]);
            END

            IF OBJECT_ID(N'[dbo].[Routiers]', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[Routiers](
                    [Id] uniqueidentifier NOT NULL,
                    [VoyageId] uniqueidentifier NOT NULL,
                    [RouteNationaleCode] nvarchar(50) NULL,
                    [NumeroLV] nvarchar(255) NULL,
                    [CreerPar] nvarchar(max) NULL,
                    [ModifierPar] nvarchar(max) NULL,
                    [CreerLe] datetime2 NOT NULL,
                    [ModifierLe] datetime2 NULL,
                    CONSTRAINT [PK_Routiers] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_Routiers_Voyages_VoyageId] FOREIGN KEY([VoyageId]) REFERENCES [dbo].[Voyages]([Id]) ON DELETE CASCADE
                );
                CREATE UNIQUE INDEX [IX_Routiers_VoyageId] ON [dbo].[Routiers]([VoyageId]);
            END

            IF OBJECT_ID(N'[dbo].[Fluviaux]', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[Fluviaux](
                    [Id] uniqueidentifier NOT NULL,
                    [VoyageId] uniqueidentifier NOT NULL,
                    [PortEmbarquementCode] nvarchar(50) NULL,
                    [PortDebarquementCode] nvarchar(50) NULL,
                    [NomNavire] nvarchar(255) NULL,
                    [TypeNavire] nvarchar(100) NULL,
                    [CreerPar] nvarchar(max) NULL,
                    [ModifierPar] nvarchar(max) NULL,
                    [CreerLe] datetime2 NOT NULL,
                    [ModifierLe] datetime2 NULL,
                    CONSTRAINT [PK_Fluviaux] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_Fluviaux_Voyages_VoyageId] FOREIGN KEY([VoyageId]) REFERENCES [dbo].[Voyages]([Id]) ON DELETE CASCADE
                );
                CREATE UNIQUE INDEX [IX_Fluviaux_VoyageId] ON [dbo].[Fluviaux]([VoyageId]);
            END
            """, cancellationToken);

        await EnsureCurrentTransportSchemaAsync(context, cancellationToken);
    }

    /// <summary>
    /// Enregistre toutes les migrations connues d'EF pour que MigrateAsync ne tente pas de rÃ©appliquer un schÃ©ma dÃ©jÃ  alignÃ© sur le modÃ¨le (EnsureCreated).
    /// </summary>
    private static async Task MarkAllEfMigrationsAsAppliedAsync(DbContext context, ILogger logger, CancellationToken cancellationToken)
    {
        const string productVersion = "8.0.10";
        try
        {
            await EnsureMigrationsHistoryTableAsync(context, cancellationToken);
            foreach (var migrationId in context.Database.GetMigrations())
            {
                await context.Database.ExecuteSqlRawAsync(
                    """
                    IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = {0})
                    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES ({0}, {1})
                    """,
                    new object[] { migrationId, productVersion },
                    cancellationToken);
            }

            logger.LogInformation("[DÃ©marrage] {Count} entrÃ©e(s) d'historique de migrations synchronisÃ©e(s).", context.Database.GetMigrations().Count());
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "[DÃ©marrage] Impossible de marquer toutes les migrations comme appliquÃ©es.");
            throw;
        }
    }

    private static string MaskConnectionStringForLog(string connectionString)
    {
        try
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            return $"Server={builder.DataSource};Database={builder.InitialCatalog};User ID={builder.UserID};(Password masquÃ©)";
        }
        catch
        {
            return "(chaÃ®ne non reconnue, masquÃ©e)";
        }
    }
}
