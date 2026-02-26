using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

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

        var connectionString = config.GetConnectionString("AssuranceConnection")
            ?? "Server=localhost,1420;Database=MS_ASSURANCE;User Id=sa;Password=DevStrongPwd@123;TrustServerCertificate=True;";
        var serverHint = MaskConnectionStringForLog(connectionString);
        app.Logger.LogInformation("[Démarrage] Création de la base et des tables si nécessaire. Connexion : {ServerHint}", serverHint);

        // 1. Créer la base (ex. MS_ASSURANCE) si elle n'existe pas
        await EnsureDatabaseExistsAsync(connectionString, app.Logger, cancellationToken);

        // 2. Créer ou mettre à jour les tables (migrations EF, ou EnsureCreated si aucune migration détectée)
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = app.Logger;
        var contextType = typeof(AssuranceService.Infrastructure.Data.AssuranceDbContext);

        for (var attempt = 1; attempt <= MaxRetries; attempt++)
        {
            try
            {
                var context = (DbContext)services.GetRequiredService(contextType);
                var pending = await context.Database.GetPendingMigrationsAsync(cancellationToken);
                var pendingList = pending.ToList();

                logger.LogInformation("[Démarrage] Migrations en attente : {Count} ({Migrations}).",
                    pendingList.Count, pendingList.Count > 0 ? string.Join(", ", pendingList) : "aucune");

                if (pendingList.Count > 0)
                {
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
                    await SeedInitialMigrationHistoryAsync(context, "20260225120000_InitialCreate", logger, cancellationToken);
                    logger.LogInformation("[Démarrage] Tables créées et historique initial enregistré.");
                    return;
                }

                logger.LogInformation("[Démarrage] Base et tables déjà à jour (aucune migration en attente).");
                return;
            }
            catch (Exception ex)
            {
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
            logger.LogWarning(ex, "[Démarrage] Impossible de créer la base (vérifiez le serveur SQL et les droits de l'utilisateur).");
            throw;
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
    /// Après EnsureCreated(), crée __EFMigrationsHistory si besoin et enregistre la migration initiale
    /// pour que les prochaines Migrate() ne réappliquent pas InitialCreate.
    /// </summary>
    private static async Task SeedInitialMigrationHistoryAsync(
        DbContext context,
        string migrationId,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        try
        {
            // EnsureCreated() ne crée pas __EFMigrationsHistory ; la créer pour pouvoir y insérer.
            await context.Database.ExecuteSqlRawAsync("""
                IF OBJECT_ID(N'[dbo].[__EFMigrationsHistory]', N'U') IS NULL
                CREATE TABLE [__EFMigrationsHistory] (
                    [MigrationId] nvarchar(150) NOT NULL,
                    [ProductVersion] nvarchar(32) NOT NULL,
                    CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
                );
                """, cancellationToken);

            // Passer les paramètres en tableau pour ne pas confondre le CancellationToken avec un paramètre SQL.
            await context.Database.ExecuteSqlRawAsync(
                "INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES ({0}, {1})",
                new object[] { migrationId, "8.0.0" },
                cancellationToken);
            logger.LogInformation("[Démarrage] Migration {MigrationId} enregistrée dans l'historique.", migrationId);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "[Démarrage] Impossible d'enregistrer la migration dans l'historique (ignoré).");
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
