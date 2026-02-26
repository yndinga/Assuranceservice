using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AssuranceService.Domain.Models.Commons;

namespace AssuranceService.Domain.Models.Referentiel;

[Table("Aeroports")]
public class Aeroport : BaseModel
{

    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Nom { get; set; } = string.Empty;

    public Guid? PaysId { get; set; }

    public bool Actif { get; set; } = true;
}
