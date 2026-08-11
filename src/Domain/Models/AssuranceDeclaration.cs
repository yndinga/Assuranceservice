using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AssuranceService.Domain.Models.Commons;

namespace AssuranceService.Domain.Models;

[Table("AssuranceDeclarations")]
public class AssuranceDeclaration : BaseModel
{
    public Guid AssuranceId { get; set; }
    public Guid DeclarationId { get; set; }

    [MaxLength(100)]
    public string? NumeroDI { get; set; }

    [MaxLength(50)]
    public string PaysEmbarquementCode { get; set; } = string.Empty;

    [MaxLength(50)]
    public string PortEmbarquementCode { get; set; } = string.Empty;

    public virtual Assurance Assurance { get; set; } = null!;
    public virtual ICollection<AssuranceLigne> Lignes { get; set; } = new List<AssuranceLigne>();
}
