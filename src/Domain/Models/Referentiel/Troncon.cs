using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AssuranceService.Domain.Models.Commons;

namespace AssuranceService.Domain.Models.Referentiel;

[Table("Troncons")]
public class Troncon : BaseModel
{
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Description { get; set; } = string.Empty;

    public Guid CorridorId { get; set; }

    public Guid RouteId { get; set; }

    public bool Actif { get; set; } = true;
}
