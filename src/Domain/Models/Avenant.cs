using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AssuranceService.Domain.Constants;
using AssuranceService.Domain.Models.Commons;

namespace AssuranceService.Domain.Models;

/// <summary>
/// <para><b>En-tête d’avenant</b> après correction sur une police déjà validée : numéro d’avenant, motif, lien police
/// (corrections portant sur la police).</para>
/// <para><b>Flux métier :</b> l’importateur ouvre l’assurance → bouton Avenant → formulaire type mise à jour prérempli
/// depuis l’assurance. Il ne modifie que les champs en erreur. À l’enregistrement :</para>
/// <list type="number">
/// <item>Mise à jour des champs <see cref="Assurance"/> (et prime si recalcul).</item>
/// <item>Création de cet enregistrement <see cref="Avenant"/> avec un nouveau <see cref="NoAvenant"/>.</item>
/// <item>Pour chaque champ modifié : une ligne dans <see cref="Historique"/> (<see cref="Historique.ValeurAvant"/> / <see cref="Historique.ValeurApres"/>).</item>
/// <item>Si un champ impacte le calcul : recalcul et mise à jour des <see cref="Prime"/> de l’assurance ; tracer les champs de prime dans <see cref="Historique"/> si besoin (<c>CibleEntite = Prime</c>).</item>
/// </list>
/// </summary>
[Table("Avenants")]
public class Avenant : BaseModel
{
    [Required]
    [Column("AssuranceId")]
    [ForeignKey(nameof(Assurance))]
    public Guid AssuranceId { get; set; }

    public virtual Assurance? Assurance { get; set; }

    [Required]
    [MaxLength(255)]
    public string NoPolice { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string NoAvenant { get; set; } = string.Empty;

    [Column(TypeName = "nvarchar(10)")]
    [MaxLength(10)]
    public string Statut { get; set; } = "10";

    [Column(TypeName = "nvarchar(max)")]
    public string? Motif { get; set; }

    /// <summary>Type ou nature de l’avenant (code métier : correction, extension, annulation, etc.).</summary>
    [Required]
    [Column("Type", TypeName = "nvarchar(50)")]
    [MaxLength(50)]
    public string Type { get; set; } = null!;

    public virtual ICollection<Historique> Historiques { get; set; } = new List<Historique>();
}
