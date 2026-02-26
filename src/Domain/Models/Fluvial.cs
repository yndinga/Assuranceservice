using System.ComponentModel.DataAnnotations.Schema;
using AssuranceService.Domain.Models.Commons;
using AssuranceService.Domain.Models.Referentiel;

namespace AssuranceService.Domain.Models;

[Table("Fluviaux")]
public class Fluvial : BaseModel
{
    public Guid PortEmbarquementId { get; set; }
    public virtual Port? PortEmbarquement { get; set; }

    public Guid? PortDebarquementId { get; set; }
    public virtual Port? PortDebarquement { get; set; }

    public Guid VoyageId { get; set; }
    public virtual Voyage? Voyage { get; set; }
}
