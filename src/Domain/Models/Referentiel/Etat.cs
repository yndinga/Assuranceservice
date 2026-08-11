using System.ComponentModel.DataAnnotations.Schema;
using AssuranceService.Domain.Models.Commons;
namespace AssuranceService.Domain.Models.Referentiel;

[Table("Etats")]
public class Etat : BaseModel
{
    public int Code { get; set; }

    public string Libelle { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string CodeEcran { get; set; } = string.Empty;

    public string UsageUI { get; set; } = string.Empty;

    public bool Actif { get; set; } = true;
}

