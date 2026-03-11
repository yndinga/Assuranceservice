namespace AssuranceService.Domain.Constants;

/// <summary>
/// Guid fixes pour les statuts d'assurance (référentiel Statuts).
/// À aligner avec les enregistrements en base.
/// </summary>
public static class StatutAssuranceIds
{
    public static readonly Guid EnAttente = Guid.Parse("00000000-0000-0000-0000-000000000001");
    public static readonly Guid Elaborer = Guid.Parse("00000000-0000-0000-0000-000000000002");
    public static readonly Guid Created = Guid.Parse("00000000-0000-0000-0000-000000000003");
    public static readonly Guid MarchandisesAdded = Guid.Parse("00000000-0000-0000-0000-000000000004");
    public static readonly Guid PrimeCalculated = Guid.Parse("00000000-0000-0000-0000-000000000005");
    public static readonly Guid VisaDemandé = Guid.Parse("00000000-0000-0000-0000-000000000006");
    public static readonly Guid Failed = Guid.Parse("00000000-0000-0000-0000-000000000007");
    public static readonly Guid Completed = Guid.Parse("00000000-0000-0000-0000-000000000008");
}
