using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AssuranceService.Domain.Models.Commons;

namespace AssuranceService.Domain.Models;

[Table("Specificites")]
public class Specificite : BaseModel
{
    [Required]
    [MaxLength(255)]
    public string Nom { get; set; } = string.Empty;
}
