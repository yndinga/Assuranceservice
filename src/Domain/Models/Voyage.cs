using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AssuranceService.Domain.Models.Commons;

namespace AssuranceService.Domain.Models;

[Table("Voyages")]
public class Voyage : BaseModel
{
    [Required]
    public Guid AssuranceId { get; set; }

    public virtual Assurance? Assurance { get; set; }

    [Required]
    [MaxLength(255)]
    public string NomTransporteur { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? LieuSejour { get; set; }

    [MaxLength(50)]
    public string? DureeSejour { get; set; }

    [Required]
    [MaxLength(255)]
    public string PaysProvenance { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string PaysDestination { get; set; } = string.Empty;

    [Column(TypeName = "nvarchar(255)")]
    [MaxLength(255)]
    public string Designation { get; set; } = string.Empty;

     [Column(TypeName = "nvarchar(255)")]
    [MaxLength(255)]
    public string? Marque { get; set; }

    [Column(TypeName = "nvarchar(500)")]
    [MaxLength(500)]
    public string? Nature { get; set; }

    [Column(TypeName = "nvarchar(100)")]
    [MaxLength(100)]
    public string? Specificites { get; set; }

    [Column(TypeName = "nvarchar(500)")]
    [MaxLength(500)]
    public string? Conditionnement { get; set; }

    [Column(TypeName = "nvarchar(500)")]
    [MaxLength(500)]
    public string? DescriptionConditionnement { get; set; }

    [Column(TypeName = "nvarchar(50)")]
    [MaxLength(50)]
    public string? Devise { get; set; }

    [Column(TypeName = "nvarchar(255)")]
    [MaxLength(255)]
    public string? MasseBrute { get; set; }

    [Column(TypeName = "nvarchar(255)")]
    [MaxLength(255)]
    public string? UniteStatistique { get; set; }

    public virtual Aerien? Aerien { get; set; }
    public virtual Maritime? Maritime { get; set; }
    public virtual Routier? Routier { get; set; }
    public virtual Fluvial? Fluvial { get; set; }
}
