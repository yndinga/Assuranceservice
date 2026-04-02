using System.ComponentModel.DataAnnotations.Schema;
using AssuranceService.Domain.Models.Commons;
using AssuranceService.Domain.Models.Referentiel;

namespace AssuranceService.Domain.Models;

[Table("Maritimes")]
public class Maritime : BaseModel
{
    public Guid PortEmbarquementId { get; set; }
    public virtual Port? PortEmbarquement { get; set; }

    public Guid PortDebarquementId { get; set; }
    public virtual Port? PortDebarquement { get; set; }

    public Guid AssuranceId { get; set; }
    public virtual Assurance? Assurance { get; set; }
}
