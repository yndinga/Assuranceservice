using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AssuranceService.Domain.Models.Commons;

namespace AssuranceService.Domain.Models.Referentiel;

[Table("Statuts")]
public class Statut : BaseModel
{
    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(250)]
    public string Libelle { get; set; } = string.Empty;

    // Audit : CreerPar, ModifierPar, CreerLe, ModifierLe hérités de BaseModel
}
