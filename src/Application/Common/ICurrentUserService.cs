namespace AssuranceService.Application.Common;

public interface ICurrentUserService
{
    /// <summary>
    /// Nom de l'utilisateur courant (audit).
    /// En l'absence d'utilisateur (ex: jobs/consumers), retourne une valeur par défaut.
    /// </summary>
    string UserName { get; }

    /// <summary>Code de l'organisation connectée (ex: OCRE d'un importateur/transitaire, ou code d'un assureur/intermédiaire).</summary>
    string OrganisationCode { get; }

    /// <summary>Type de l'organisation connectée (ex: IMPORTATEUR, TRANSITAIRE, ASSUREUR, COURTIER, AGENT_GENERAL).</summary>
    string OrganisationType { get; }
}

