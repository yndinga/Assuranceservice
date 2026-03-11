namespace AssuranceService.Domain.Constants;

/// <summary>
/// Codes statuts d'assurance (référentiel Statuts, table Statuts).
/// Aligné avec Insert_Statuts_Referentiel.sql (10=Elaboré, 11=Visa demandé, ...).
/// </summary>
public static class StatutAssuranceCodes
{
    /// <summary>Elaboré — statut par défaut à la création.</summary>
    public const string Elaboré = "10";

    public const string VisaDemandé = "11";
    public const string ModificationSoumise = "12";
    public const string Validé = "13";
    public const string ModificationDemandée = "14";
    public const string AttenteValidation = "15";
    public const string Payé = "16";
    public const string Approuvé = "17";
}
