namespace AssuranceService.Application.Avenants.Commands;

/// <summary>Corps JSON pour POST avenant (l’<see cref="EnregistrerAvenantCommand.AssuranceId"/> vient de la route).</summary>
public class EnregistrerAvenantBody
{
    public string Type { get; set; } = string.Empty;
    public string Motif { get; set; } = string.Empty;
    public AvenantAssurancePatch? Assurance { get; set; }
}
