namespace AssuranceService.Domain.Constants;

public static class AvenantTypes
{
    public const string Modification = "MODIFICATION";
    public const string Prorogation = "PROROGATION";
    public const string Annulation = "ANNULATION";

    public static readonly IReadOnlySet<string> Allowed = new HashSet<string>
    {
        Modification,
        Prorogation,
        Annulation
    };
}

