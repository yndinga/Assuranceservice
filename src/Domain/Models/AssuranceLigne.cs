using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AssuranceService.Domain.Models.Commons;

namespace AssuranceService.Domain.Models;

[Table("AssuranceLignes")]
public class AssuranceLigne : BaseModel
{
    public Guid AssuranceId { get; set; }
    public Guid AssuranceDeclarationId { get; set; }
    public Guid DeclarationId { get; set; }
    public Guid SourceCommandeId { get; set; }
    public Guid SourceLigneDIId { get; set; }
    public int NoLigne { get; set; }

    [MaxLength(11)]
    public string PositionTarifaire { get; set; } = string.Empty;

    [MaxLength(250)]
    public string Designation { get; set; } = string.Empty;

    [MaxLength(250)]
    public string Marque { get; set; } = string.Empty;

    [MaxLength(250)]
    public string Colisage { get; set; } = string.Empty;

    public decimal? Quantite { get; set; }
    public decimal? MasseBrute { get; set; }
    public decimal? MasseNette { get; set; }
    public decimal? Volume { get; set; }
    public decimal? PrixUnitaire { get; set; }
    public decimal? ValeurDevise { get; set; }
    public decimal? ValeurXAF { get; set; }

    [MaxLength(20)]
    public string? Devise { get; set; }

    [MaxLength(50)]
    public string? UniteStatistique { get; set; }

    [MaxLength(50)]
    public string? PaysOrigine { get; set; }

    public bool EstPartielle { get; set; }

    public virtual Assurance Assurance { get; set; } = null!;
    public virtual AssuranceDeclaration AssuranceDeclaration { get; set; } = null!;
}
