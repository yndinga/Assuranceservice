using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AssuranceService.Domain.Models.Commons;

namespace AssuranceService.Domain.Models.Referentiel;

[Table("TauxDeChanges")]
public class TauxDeChange : BaseModel
{
    public Guid DeviseId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Source { get; set; } = string.Empty;

    [Column(TypeName = "decimal(20, 5)")]
    public decimal Taux { get; set; }

    [MaxLength(50)]
    public string? ValideDe { get; set; }

    public bool Actif { get; set; } = true;
}
