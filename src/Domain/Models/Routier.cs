using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AssuranceService.Domain.Models.Commons;

namespace AssuranceService.Domain.Models;

[Table("Routiers")]
public class Routier : BaseModel
{
    [MaxLength(50)]
    public string? RouteNationaleCode { get; set; }

    /// <summary>Numéro de lettre de voiture — routier uniquement.</summary>
    [MaxLength(255)]
    public string? NumeroLV { get; set; }

    [Required]
    public Guid VoyageId { get; set; }
    public virtual Voyage? Voyage { get; set; }
}
