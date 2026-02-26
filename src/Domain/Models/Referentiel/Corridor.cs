using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AssuranceService.Domain.Models.Commons;

namespace AssuranceService.Domain.Models.Referentiel;

[Table("Corridors")]
public class Corridor : BaseModel
{

    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Nom { get; set; } = string.Empty;

    public bool Actif { get; set; } = true;
}
