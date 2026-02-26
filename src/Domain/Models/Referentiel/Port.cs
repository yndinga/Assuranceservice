using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AssuranceService.Domain.Models.Commons;

namespace AssuranceService.Domain.Models.Referentiel;

/// <summary>
/// Ports maritimes et fluviaux (table unique).
/// Type = "MA" (Maritime) ou "FL" (Fluvial).
/// </summary>
[Table("Ports")]
public class Port : BaseModel
{
    public string Code { get; set; } = string.Empty;

    [Required]
    public string Nom { get; set; } = string.Empty;

    public Guid? PaysId { get; set; }

    public string Module { get; set; } = string.Empty;

    public bool Actif { get; set; } = true;

    /// <summary>MA = Maritime, FL = Fluvial</summary>
    [MaxLength(2)]
    public string? Type { get; set; }
}
