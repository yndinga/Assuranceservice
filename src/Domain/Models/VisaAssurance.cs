using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AssuranceService.Domain.Models.Commons;

namespace AssuranceService.Domain.Models;


[Table("VisaAssurances")]
public class VisaAssurance : BaseModel
{
    [Required]
    public Guid AssuranceId { get; set; }
    public virtual Assurance? Assurance { get; set; }
    // On écrit dans cette table uniquement lors de la signature — signataire (assureur ou courtier selon le flux)
    [Required]
    public Guid OrganisationId { get; set; }

    public bool VisaOK { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string? VisaContent { get; set; }
}

