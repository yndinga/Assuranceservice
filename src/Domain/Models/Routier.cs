using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AssuranceService.Domain.Models.Commons;

namespace AssuranceService.Domain.Models;

[Table("Routiers")]
public class Routier : BaseModel
{
    [Required]
    [MaxLength(255)]
    public string RouteNationale { get; set; } = string.Empty;

    [Required]
    public Guid AssuranceId { get; set; }
    public virtual Assurance? Assurance { get; set; }
}
