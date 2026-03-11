using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AssuranceService.Domain.Models.Commons;

namespace AssuranceService.Domain.Models;

[Table("TypeTransports")]
public class TypeTransport : BaseModel
{
    [Required]
    [MaxLength(255)]
    public string Nom { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Module { get; set; } = string.Empty;
}
