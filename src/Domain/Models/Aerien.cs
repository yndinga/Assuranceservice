using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AssuranceService.Domain.Models.Commons;

namespace AssuranceService.Domain.Models;

[Table("Aeriens")]
public class Aerien : BaseModel
{
    [Required]
    [MaxLength(255)]
    public string AeroportEmbarquement { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? AeroportDebarquement { get; set; }

    [Required]
    public Guid VoyageId { get; set; }
    public virtual Voyage? Voyage { get; set; }
}
