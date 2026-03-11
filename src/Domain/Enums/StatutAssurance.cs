namespace AssuranceService.Domain.Enums;

/// <summary>
/// Statut du workflow d'assurance. Stocké en base en int.
/// </summary>
public enum StatutAssurance
{
    EnAttente = 0,
    Elaborer = 1,
    Created = 2,
    MarchandisesAdded = 3,
    PrimeCalculated = 4,
    VisaDemandé = 5,
    Failed = 6,
    Completed = 7
}
