using System.ComponentModel.DataAnnotations.Schema;
using AssuranceService.Domain.Models.Commons;
using AssuranceService.Domain.Models.Referentiel;

namespace AssuranceService.Domain.Models;

public class Voyage : BaseModel
{
    public Guid AssuranceId { get; set; }
    public Assurance? Assurance { get; set; }

    public Guid? ModuleId { get; set; }
    public virtual Module? Module { get; set; }

    public string NomTransporteur { get; set; } = string.Empty;
    public string NomNavire { get; set; } = string.Empty;
    public string TypeNavire { get; set; } = string.Empty;
    public string? LieuSejour { get; set; }
    public string? DureeSejour { get; set; }
    public string PaysProvenance { get; set; } = string.Empty;
    public string PaysDestination { get; set; } = string.Empty;

    // Tables enfants — un seul sera rempli selon le type de transport (Maritime, Aerien, Routier, Fluvial)
    public virtual Maritime? Maritime { get; set; }
    public virtual Aerien? Aerien { get; set; }
    public virtual Routier? Routier { get; set; }
    public virtual Fluvial? Fluvial { get; set; }
    
}
