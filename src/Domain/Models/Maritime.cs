using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AssuranceService.Domain.Models.Commons;

namespace AssuranceService.Domain.Models;

[Table("Maritimes")]
public class Maritime : BaseModel
{
    [MaxLength(50)]
    public string? PortEmbarquementCode { get; set; }

    [MaxLength(50)]
    public string? PortDebarquementCode { get; set; }

    /// <summary>Numéro de connaissement (Bill of Lading) — maritime uniquement.</summary>
    [MaxLength(255)]
    public string? NumeroBL { get; set; }

    [MaxLength(255)]
    public string? NomNavire { get; set; }

    [MaxLength(100)]
    public string? TypeNavire { get; set; }

    public Guid VoyageId { get; set; }
    public virtual Voyage? Voyage { get; set; }
}
