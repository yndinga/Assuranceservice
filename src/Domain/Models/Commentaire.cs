using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AssuranceService.Domain.Models.Commons;

namespace AssuranceService.Domain.Models;

/// <summary>
/// Motif ou commentaire laissé par le partenaire (ex. lors d'un rejet d'assurance ou d'avenant).
/// Historique : plusieurs lignes possibles pour le même document concerné.
/// </summary>
[Table("Commentaires")]
public class Commentaire : BaseModel
{
    /// <summary>
    /// Identifiant du document métier concerné (ex. <see cref="Assurance.Id"/> ou <see cref="Avenant.Id"/> selon le flux).
    /// Ne pas confondre avec l'entité <see cref="Document"/> (pièce jointe / fichier).
    /// </summary>
    [Required]
    [Column("DocumentId")]
    public Guid DocumentId { get; set; }

    /// <summary>Texte du motif ou commentaire (obligatoire à la création métier).</summary>
    [Required]
    [Column("motif", TypeName = "nvarchar(max)")]
    public string Motif { get; set; } = string.Empty;
}
