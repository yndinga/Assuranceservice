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

    /// <summary>
    /// Contenu du visa : rempli côté serveur à partir du token récupéré après détection de l'ID du signataire.
    /// Le backend détecte l'ID du signataire (connecté), fait une requête pour récupérer le token qui contient notamment :
    /// - NoPolice, Organisation, Partenaire, VisaOK, PartenaireId, Timestamp ;
    /// - Nom de celui qui a signé ;
    /// - Le certificat appartient à : Nom, Organisation, Pays, Valide (période).
    /// Exemple certificat : Nom=Philippe MALONGA, Organisation=DIRECTION DEPARTEMENTALE DU COMMERCE INTERIEUR, Pays=CG, Valide=2014-2017
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? VisaContent { get; set; }
}

