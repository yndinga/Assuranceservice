using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AssuranceService.Domain.Models.Commons;

namespace AssuranceService.Domain.Models;

public class Voyage : BaseModel
{
    public Guid AssuranceId { get; set; }
    public Assurance? Assurance { get; set; }

    /// <summary>Code du type de transport (stocké directement, pas d'ID ni FK vers Modules) : MA, AE, RO, FL.</summary>
    [Required]
    [MaxLength(10)]
    [Column(TypeName = "nvarchar(10)")]
    public string ModuleCode { get; set; } = string.Empty;

    public string NomTransporteur { get; set; } = string.Empty;
    public string NomNavire { get; set; } = string.Empty;
    public string TypeNavire { get; set; } = string.Empty;
    /// <summary>Défini par l'importateur, peut être null.</summary>
    public string? LieuSejour { get; set; }
    /// <summary>Défini par l'importateur, peut être null.</summary>
    public string? DureeSejour { get; set; }
    public string PaysProvenance { get; set; } = string.Empty;
    public string PaysDestination { get; set; } = string.Empty;

    // Tables enfants — un seul sera rempli selon le type de transport (Maritime, Aerien, Routier, Fluvial)
    public virtual Maritime? Maritime { get; set; }
    public virtual Aerien? Aerien { get; set; }
    public virtual Routier? Routier { get; set; }
    public virtual Fluvial? Fluvial { get; set; }
    
}
