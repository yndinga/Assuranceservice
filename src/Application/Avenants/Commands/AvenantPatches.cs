namespace AssuranceService.Application.Avenants.Commands;

/// <summary>Champs assurance modifiables via avenant (null = ne pas modifier).</summary>
public class AvenantAssurancePatch
{
    public string? ImportateurNom { get; set; }
    public string? ImportateurNIU { get; set; }
    public string? NoPolice { get; set; }
    public string? NumeroCert { get; set; }
    public DateTime? DateDebut { get; set; }
    public DateTime? DateFin { get; set; }
    public string? TypeContrat { get; set; }
    public string? Duree { get; set; }
    public string? Module { get; set; }
    public Guid? GarantieId { get; set; }
}

