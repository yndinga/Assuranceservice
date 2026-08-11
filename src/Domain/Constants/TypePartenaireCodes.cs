namespace AssuranceService.Domain.Constants;

/// <summary>
/// Codes référentiel TypePartenaires (table TypePartenaires) désignant le rôle de l'organisation
/// connectée vis-à-vis d'une assurance : Assureur (porte le contrat), Courtier / Agent général (intermédiaires).
/// </summary>
public static class TypePartenaireCodes
{
    public const string Assureur = "ASSUREUR";
    public const string Courtier = "COURTIER";
    public const string AgentGeneral = "AGENT_GENERAL";

    public static readonly IReadOnlySet<string> Intermediaires = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Courtier,
        AgentGeneral
    };
}
