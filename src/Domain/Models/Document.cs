using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AssuranceService.Domain.Models.Commons;

namespace AssuranceService.Domain.Models;

/// <summary>
/// Métadonnées d'un fichier stocké dans MinIO, lié à une assurance.
/// Le fichier physique est dans MinIO ; ici on garde l'id assurance, le nom du fichier et la clé d'accès (objectKey).
/// </summary>
[Table("Documents")]
public class Document : BaseModel
{
    public Guid AssuranceId { get; set; }
    public virtual Assurance? Assurance { get; set; }

    /// <summary>Nom du fichier tel qu'enregistré (ex: facture.pdf)</summary>
    [Required]
    [MaxLength(255)]
    public string Description { get; set; } = string.Empty;

    /// <summary>Clé de l'objet dans MinIO (ex: assurances/{assuranceId}/{guid}.pdf)</summary>
    [Required]
    [MaxLength(500)]
    public string DocumentUrl { get; set; } = string.Empty;

    /// <summary>Type MIME du fichier (ex: application/pdf).</summary>
    [MaxLength(100)]
    public string? ContentType { get; set; }

    /// <summary>Taille du fichier en octets.</summary>
    public long? Taille { get; set; }
}
