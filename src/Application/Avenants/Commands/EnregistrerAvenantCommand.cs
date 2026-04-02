using AssuranceService.Application.Avenants.DTOs;
using MediatR;

namespace AssuranceService.Application.Avenants.Commands;

public class EnregistrerAvenantCommand : IRequest<EnregistrerAvenantResponse>
{
    public Guid AssuranceId { get; set; }
    public string Motif { get; set; } = string.Empty;
    public AvenantAssurancePatch? Assurance { get; set; }
}
