using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AssuranceService.Domain.Models.Commons;

namespace AssuranceService.Domain.Models;

[Table("VisaAssurances")]
public class VisaAssurance : BaseModel
{
    [Required]
    public Guid AssuranceId { get; set; }

    public virtual Assurance? Assurance { get; set; }

    [Required]
    [MaxLength(50)]
    public string TypePartenaire { get; set; } = string.Empty;

    [Required]
    [MaxLength(250)]
    public string Organisation { get; set; } = string.Empty;

    public bool VisaOK { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string? VisaContent { get; set; }

    [MaxLength(1000)]
    public string? Message { get; set; }

    [MaxLength(250)]
    public string? Licence { get; set; }

    [Column(TypeName = "datetime2(0)")]
    public DateTime? DateVisa { get; set; }

    [Required]
    [MaxLength(50)]
    public string Statut { get; set; } = string.Empty;
}
