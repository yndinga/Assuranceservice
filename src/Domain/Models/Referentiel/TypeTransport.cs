using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AssuranceService.Domain.Models.Commons;

namespace AssuranceService.Domain.Models.Referentiel;

/// <summary>Modes de transport (MA, AR, RO, FL) — synchronisés depuis ReferentielService ModeDeTransports.</summary>
[Table("TypeTransports")]
public class TypeTransport : BaseModel
{
    [Required]
    [MaxLength(3)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Nom { get; set; } = string.Empty;

    public bool Actif { get; set; } = true;
}
