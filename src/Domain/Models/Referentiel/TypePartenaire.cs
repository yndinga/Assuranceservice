using System.ComponentModel.DataAnnotations;
using AssuranceService.Domain.Models.Commons;

namespace AssuranceService.Domain.Models.Referentiel;

public class TypePartenaire : BaseModel
{
    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string Libelle { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool Actif { get; set; } = true;
}
