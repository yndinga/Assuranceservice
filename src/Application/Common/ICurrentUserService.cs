namespace AssuranceService.Application.Common;

public interface ICurrentUserService
{
    /// <summary>
    /// Nom de l'utilisateur courant (audit).
    /// En l'absence d'utilisateur (ex: jobs/consumers), retourne une valeur par défaut.
    /// </summary>
    string UserName { get; }
}

