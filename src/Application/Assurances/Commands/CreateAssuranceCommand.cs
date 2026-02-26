using MediatR;

namespace AssuranceService.Application.Assurances.Commands;

public record CreateAssuranceCommand : IRequest<Guid>
{
    // NoPolice et NumeroCert sont générés automatiquement lors de la soumission (Étape 4)
    public string ImportateurNom { get; init; } = string.Empty;
    public string? ImportateurNIU { get; init; }
    public DateTime? DateDebut { get; init; }
    public DateTime? DateFin { get; init; }
    public string TypeContrat { get; init; } = string.Empty;
    public string? Duree { get; init; }
    // Statut géré par le système — toujours "Elaborer" à la création
    public Guid? AssureurId { get; init; }
    public Guid? IntermediaireId { get; init; }
    public Guid? GarantieId { get; init; }
    public string? OCRE { get; init; }
}



