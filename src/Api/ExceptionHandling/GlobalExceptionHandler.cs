using System.Net;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AssuranceService.Api.ExceptionHandling;

/// <summary>
/// Gestionnaire global : transforme toutes les exceptions non gérées en réponses JSON lisibles.
/// Enregistrer APRÈS ValidationExceptionHandler dans Program.cs.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        HttpStatusCode status;
        string title;
        string detail;

        switch (exception)
        {
            case ArgumentException ex:
                status = HttpStatusCode.BadRequest;
                title  = "Requête invalide";
                detail = ex.Message;
                _logger.LogWarning(ex, "{Title} : {Message}", title, ex.Message);
                break;

            case InvalidOperationException ex:
                status = HttpStatusCode.UnprocessableEntity;
                title  = "Opération impossible";
                detail = ex.Message;
                _logger.LogWarning(ex, "{Title} : {Message}", title, ex.Message);
                break;

            case KeyNotFoundException ex:
                status = HttpStatusCode.NotFound;
                title  = "Ressource introuvable";
                detail = ex.Message;
                _logger.LogWarning(ex, "{Title} : {Message}", title, ex.Message);
                break;

            case UnauthorizedAccessException ex:
                status = HttpStatusCode.Unauthorized;
                title  = "Accès non autorisé";
                detail = ex.Message;
                _logger.LogWarning(ex, "{Title} : {Message}", title, ex.Message);
                break;

            case DbUpdateException ex:
                var sqlDetail = ex.InnerException?.Message ?? ex.Message;
                if (IsPortForeignKeyError(sqlDetail))
                {
                    status = HttpStatusCode.UnprocessableEntity;
                    title  = "Données invalides";
                    detail = "Le port n'existe pas.";
                }
                else if (IsDataTypeConversionError(sqlDetail))
                {
                    status = HttpStatusCode.UnprocessableEntity;
                    title  = "Données invalides";
                    detail = GetConversionErrorMessage(sqlDetail);
                }
                else
                {
                    status = HttpStatusCode.Conflict;
                    title  = "Erreur de base de données";
                    detail = _env.IsDevelopment()
                        ? $"{ex.Message} | Détail SQL : {sqlDetail}"
                        : DiagnosticDbMessage(sqlDetail);
                }
                _logger.LogError(ex, "DbUpdateException : {SqlDetail}", sqlDetail);
                break;

            case SqlException ex:
                var sqlExDetail = ex.Message ?? string.Empty;
                if (IsMissingColumnError(sqlExDetail))
                {
                    status = HttpStatusCode.UnprocessableEntity;
                    title = "Schéma base non aligné";
                    detail = "La base de données n'est pas à jour avec l'application (colonne manquante). Exécutez les scripts/migrations de mise à jour puis redémarrez l'API.";
                }
                else
                {
                    status = HttpStatusCode.Conflict;
                    title = "Erreur SQL";
                    detail = _env.IsDevelopment()
                        ? $"SqlException : {sqlExDetail}"
                        : DiagnosticDbMessage(sqlExDetail);
                }
                _logger.LogError(ex, "SqlException : {SqlDetail}", sqlExDetail);
                break;

            default:
                status = HttpStatusCode.InternalServerError;
                title  = "Erreur interne du serveur";
                detail = _env.IsDevelopment()
                    ? $"{exception.GetType().Name} : {exception.Message}"
                    : "Une erreur interne s'est produite. Veuillez contacter l'administrateur.";
                _logger.LogError(exception, "Erreur inattendue : {Message}", exception.Message);
                break;
        }

        var problem = new ProblemDetails
        {
            Status = (int)status,
            Title  = title,
            Detail = detail
        };

        httpContext.Response.StatusCode = (int)status;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

        return true;
    }

    /// <summary>
    /// Détecte une erreur de conversion de type (ex. nvarchar vers numeric).
    /// </summary>
    private static bool IsDataTypeConversionError(string sqlMessage)
    {
        if (string.IsNullOrEmpty(sqlMessage)) return false;
        return sqlMessage.Contains("converting data type", StringComparison.OrdinalIgnoreCase) ||
               sqlMessage.Contains("conversion failed", StringComparison.OrdinalIgnoreCase) ||
               sqlMessage.Contains("nvarchar to numeric", StringComparison.OrdinalIgnoreCase) ||
               sqlMessage.Contains("varchar to numeric", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Message clair pour les erreurs de conversion de type.
    /// </summary>
    private static string GetConversionErrorMessage(string sqlMessage)
    {
        if (sqlMessage.Contains("nvarchar to numeric", StringComparison.OrdinalIgnoreCase) ||
            sqlMessage.Contains("varchar to numeric", StringComparison.OrdinalIgnoreCase))
            return "Une valeur texte a été fournie pour un champ numérique. Vérifiez les champs (ex. Taux de garantie : envoyer un nombre comme 0.002, pas une chaîne). Si la base a été migrée récemment, redémarrez l'application et réessayez.";

        return "Format de donnée invalide : un champ attend un type différent (ex. nombre au lieu de texte). Vérifiez les valeurs envoyées.";
    }

    /// <summary>
    /// Détecte une erreur de clé étrangère sur la table Ports (port introuvable).
    /// </summary>
    private static bool IsPortForeignKeyError(string sqlMessage)
    {
        if (string.IsNullOrEmpty(sqlMessage)) return false;
        return (sqlMessage.Contains("FK_Maritimes_Ports", StringComparison.OrdinalIgnoreCase) ||
                (sqlMessage.Contains("FOREIGN KEY", StringComparison.OrdinalIgnoreCase) &&
                 sqlMessage.Contains("Ports", StringComparison.OrdinalIgnoreCase)));
    }

    private static bool IsMissingColumnError(string sqlMessage)
    {
        if (string.IsNullOrEmpty(sqlMessage)) return false;
        return sqlMessage.Contains("Invalid column name", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Traduit les messages SQL courants en messages métier lisibles (pour la production).
    /// </summary>
    private static string DiagnosticDbMessage(string sqlMessage)
    {
        if (sqlMessage.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase) ||
            sqlMessage.Contains("duplicate", StringComparison.OrdinalIgnoreCase) ||
            sqlMessage.Contains("Cannot insert duplicate", StringComparison.OrdinalIgnoreCase))
            return "Un enregistrement avec ces informations existe déjà (contrainte d'unicité).";

        if (sqlMessage.Contains("FOREIGN KEY", StringComparison.OrdinalIgnoreCase) ||
            sqlMessage.Contains("foreign key constraint", StringComparison.OrdinalIgnoreCase))
            return "Référence invalide : l'enregistrement lié est introuvable.";

        if (sqlMessage.Contains("NULL", StringComparison.OrdinalIgnoreCase) ||
            sqlMessage.Contains("cannot be null", StringComparison.OrdinalIgnoreCase))
            return "Un champ obligatoire est manquant.";

        if (sqlMessage.Contains("Invalid column name", StringComparison.OrdinalIgnoreCase) ||
            sqlMessage.Contains("column", StringComparison.OrdinalIgnoreCase))
            return "Erreur de structure de base de données. Une migration est peut-être manquante.";

        if (sqlMessage.Contains("converting data type", StringComparison.OrdinalIgnoreCase) ||
            sqlMessage.Contains("conversion failed", StringComparison.OrdinalIgnoreCase))
            return "Une valeur a un format incorrect pour un champ (ex. texte au lieu d'un nombre). Vérifiez les champs Taux, montants, etc.";

        return "Une erreur de base de données s'est produite.";
    }
}
