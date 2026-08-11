using AssuranceService.Application.Avenants.DTOs;
using MediatR;

namespace AssuranceService.Application.Avenants.Commands;

public class EnregistrerAvenantCommand : IRequest<EnregistrerAvenantResponse>
{
    public Guid AssuranceId { get; set; }

    /// <summary>Code du type d’avenant (ex. MODIFICATION, PROROGATION, ANNULATION).</summary>
    public string Type { get; set; } = string.Empty;

    public string Motif { get; set; } = string.Empty;
    public AvenantAssurancePatch? Assurance { get; set; }
}
