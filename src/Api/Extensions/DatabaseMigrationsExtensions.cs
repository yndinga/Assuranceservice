using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace AssuranceService.Api.Extensions;

/// <summary>
/// Au démarrage : crée la base de données si elle n'existe pas, puis crée/met à jour toutes les tables (migrations EF).
/// Désactiver avec ApplyMigrationsAtStartup=false dans la configuration.
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
            app.Logger.LogInformation("[Démarrage] ApplyMigrationsAtStartup=false : pas de création de base ni de tables.");
            return;
        }

        var connectionString = NormalizeSqlServerConnectionString(
            config.GetConnectionString("AssuranceConnection")
            ?? "Server=localhost,1420;Database=MS_ASSURANCE;User Id=sa;Password=DevStrongPwd@123;TrustServerCertificate=True;");
        var serverHint = MaskConnectionStringForLog(connectionString);
        app.Logger.LogInformation("[Démarrage] Création de la base et des tables si nécessaire. Connexion : {ServerHint}", serverHint);

        var logger = app.Logger;
        var contextType = typeof(AssuranceService.Infrastructure.Data.AssuranceDbContext);

        for (var attempt = 1; attempt <= MaxRetries; attempt++)
        {
            try
            {
                // 1. Créer la base sur master (même boucle de tentatives que les migrations)
                await EnsureDatabaseExistsAsync(connectionString, logger, cancellationToken);

                // 2. Créer ou mettre à jour les tables (migrations EF, ou EnsureCreated si besoin)
                using var scope = app.Services.CreateScope();
                var services = scope.ServiceProvider;
                var context = (DbContext)services.GetRequiredService(contextType);
                var pending = await context.Database.GetPendingMigrationsAsync(cancellationToken);
                var pendingList = pending.ToList();

                logger.LogInformation("[Démarrage] Migrations en attente : {Count} ({Migrations}).",
                    pendingList.Count, pendingList.Count > 0 ? string.Join(", ", pendingList) : "aucune");

                if (pendingList.Count > 0)
                {
                    // EF ne détecte parfois que les migrations ayant un .Designer.cs : la 1ère migration appliquée
                    // peut alors être une migration « incrémentale » alors que la base est vide (pas de table Primes, etc.).
                    if (!await AssurancesTableExistsAsync(context, cancellationToken))
                    {
                        logger.LogWarning(
                            "[Démarrage] Table métier [Assurances] absente alors que {Count} migration(s) sont en attente. " +
                            "Création du schéma depuis le modèle actuel (EnsureCreated) puis enregistrement de toutes les migrations connues d'EF dans __EFMigrationsHistory.",
                            pendingList.Count);
                        await context.Database.EnsureCreatedAsync(cancellationToken);
                        await EnsureMigrationsHistoryTableAsync(context, cancellationToken);
                        await MarkAllEfMigrationsAsAppliedAsync(context, logger, cancellationToken);
                        logger.LogInformation("[Démarrage] Schéma et historique des migrations synchronisés avec le modèle EF.");
                        return;
                    }

                    await context.Database.MigrateAsync(cancellationToken);
                    logger.LogInformation("[Démarrage] Migrations appliquées. Base et tables à jour.");
                    return;
                }

                // Fallback : si EF ne détecte aucune migration, créer les tables au démarrage avec EnsureCreated()
                // sauf si le schéma est déjà en place (historique présent ET au moins une table métier).
                var canConnect = await context.Database.CanConnectAsync(cancellationToken);
                if (!canConnect)
                {
                    logger.LogWarning("[Démarrage] Impossible de se connecter à la base.");
                    throw new InvalidOperationException("Impossible de se connecter à la base.");
                }

                var historyExists = await HistoryTableExistsAsync(context, cancellationToken);
                if (!historyExists)
                {
                    logger.LogInformation("[Démarrage] Aucune table d'historique : création des tables au démarrage (EnsureCreated).");
                    await context.Database.EnsureCreatedAsync(cancellationToken);
                    await EnsureMigrationsHistoryTableAsync(context, cancellationToken);
                    await MarkAllEfMigrationsAsAppliedAsync(context, logger, cancellationToken);
                    logger.LogInformation("[Démarrage] Tables créées et toutes les migrations EF enregistrées dans l'historique.");
                    return;
                }

                logger.LogInformation("[Démarrage] Base et tables déjà à jour (aucune migration en attente).");
                return;
            }
            catch (Exception ex)
            {
                LogSqlConnectionTroubleshooting(logger, ex);
                logger.LogWarning(ex,
                    "[Démarrage] Tentative {Attempt}/{Max} échouée. Réessai dans {Delay}s. Connexion : {ServerHint}",
                    attempt, MaxRetries, RetryDelay.TotalSeconds, serverHint);

                if (attempt == MaxRetries)
                {
                    logger.LogError(
                        "[Démarrage] Échec après {Max} tentatives. Pour démarrer sans créer la base/tables : ApplyMigrationsAtStartup=false. Connexion : {ServerHint}",
                        MaxRetries, serverHint);
                    logger.LogError(ex, "[Démarrage] Détail de l'exception.");
                    throw;
                }

                await Task.Delay(RetryDelay, cancellationToken);
            }
        }
    }

    /// <summary>
    /// Se connecte à master et crée la base si elle n'existe pas (évite erreur 4060 "Cannot open database").
    /// </summary>
    private static async Task EnsureDatabaseExistsAsync(string connectionString, ILogger logger, CancellationToken cancellationToken)
    {
        try
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            var databaseName = builder.InitialCatalog;
            if (string.IsNullOrWhiteSpace(databaseName))
            {
                logger.LogWarning("[Démarrage] Aucune base (Initial Catalog) dans la chaîne de connexion.");
                return;
            }

            builder.InitialCatalog = "master";
            // Ne pas assigner null à AttachDBFilename (ArgumentNullException) ; utiliser Empty pour ne pas attacher de fichier
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
            logger.LogInformation("[Démarrage] Base {Database} vérifiée ou créée.", databaseName);
        }
        catch (Exception ex)
        {
            LogSqlConnectionTroubleshooting(logger, ex);
            logger.LogWarning(ex, "[Démarrage] Impossible de créer ou d'atteindre la base sur le serveur SQL (voir message détaillé ci-dessus).");
            throw;
        }
    }

    /// <summary>
    /// Corrige des erreurs fréquentes (ex. espace après la virgule du port : "localhost, 1420").
    /// </summary>
    private static string NormalizeSqlServerConnectionString(string connectionString)
    {
        try
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            if (!string.IsNullOrEmpty(builder.DataSource))
            {
                // "host, 1433" → "host,1433"
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

        // 10061 : connexion refusée ; -2 : timeout
        if (sql.Number is 10061 or 11001 or -2)
        {
            logger.LogWarning(
                "[Démarrage] SQL Server inaccessible (erreur {Number}). Contrôlez : le service SQL est démarré ; " +
                "le port TCP dans ConnectionStrings correspond à celui configuré (souvent 1433 pour l’instance par défaut) ; " +
                "TCP/IP est activé dans le Gestionnaire de configuration SQL Server ; " +
                "pas d’espace après la virgule dans Server=machine,port.",
                sql.Number);
        }
    }

    /// <summary>
    /// Vérifie si la table __EFMigrationsHistory existe (migrations déjà utilisées).
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
    /// Crée la table __EFMigrationsHistory si elle n'existe pas (EnsureCreated ne la crée pas).
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
    /// Indique si la table Assurances existe (repère qu'un schéma métier a déjà été créé par migrations initiales).
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
    /// Enregistre toutes les migrations connues d'EF pour que MigrateAsync ne tente pas de réappliquer un schéma déjà aligné sur le modèle (EnsureCreated).
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

            logger.LogInformation("[Démarrage] {Count} entrée(s) d'historique de migrations synchronisée(s).", context.Database.GetMigrations().Count());
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "[Démarrage] Impossible de marquer toutes les migrations comme appliquées.");
            throw;
        }
    }

    private static string MaskConnectionStringForLog(string connectionString)
    {
        try
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            return $"Server={builder.DataSource};Database={builder.InitialCatalog};User ID={builder.UserID};(Password masqué)";
        }
        catch
        {
            return "(chaîne non reconnue, masquée)";
        }
    }
}
