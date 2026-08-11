using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AssuranceService.Domain.Models.Commons;

namespace AssuranceService.Domain.Models;

[Table("Aeriens")]
public class Aerien : BaseModel
{
    [MaxLength(50)]
    public string? AeroportEmbarquementCode { get; set; }

    [MaxLength(50)]
    public string? AeroportDebarquementCode { get; set; }

    /// <summary>Numéro de lettre de transport aérien (LTA / AWB) — aérien uniquement.</summary>
    [MaxLength(255)]
    public string? NumeroLTA { get; set; }

    [Required]
    public Guid VoyageId { get; set; }
    public virtual Voyage? Voyage { get; set; }
}
