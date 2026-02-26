using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AssuranceService.Domain.Models.Commons;

namespace AssuranceService.Domain.Models.Referentiel;

[Table("Departements")]
public class Departement : BaseModel
{

    public string Code { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;

    public int Ordre { get; set; }

    public bool Actif { get; set; } = true;
}
