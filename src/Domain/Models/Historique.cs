using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AssuranceService.Domain.Models.Commons;

namespace AssuranceService.Domain.Models;

/// <summary>
/// <para>Une ligne d’historique pour une <b>correction par avenant</b> : un champ modifié sur l’assurance
/// ou sur la prime, avec <b>valeur avant</b> et <b>valeur après</b> (texte sérialisé si besoin).</para>
/// <para><b>Flux :</b> police validée → constat d’erreur → formulaire « Avenant » prérempli depuis l’assurance →
/// l’importateur corrige → à l’enregistrement le système met à jour <see cref="Assurance"/> / prime,
/// crée l’en-tête <see cref="Avenant"/> (numéro d’avenant), et insère une ligne <see cref="Historique"/> par champ modifié.
/// Si un champ impacte le calcul, recalcul des <see cref="Prime"/> sur l’assurance ; les changements de prime peuvent être tracés avec <c>CibleEntite = HistoriqueCibles.Prime</c>.</para>
/// <para><see cref="AssuranceId"/> est recopié depuis l’avenant pour filtrer tout l’historique d’une police sans jointure obligatoire.</para>
/// </summary>
[Table("Historiques")]
public class Historique : BaseModel
{
    /// <summary>Police concernée (même valeur que <see cref="Avenant.AssuranceId"/> ; pas de navigation pour éviter les doubles chemins de cascade SQL).</summary>
    [Required]
    public Guid AssuranceId { get; set; }

    /// <summary>Avenant auquel rattacher ce lot de modifications (même enregistrement « session » correction).</summary>
    [Required]
    public Guid AvenantId { get; set; }

    public virtual Avenant? Avenant { get; set; }

    /// <summary>Portée du champ : <see cref="Constants.HistoriqueCibles"/>.</summary>
    [Required]
    [MaxLength(30)]
    public string CibleEntite { get; set; } = string.Empty;

    /// <summary>Identifiant de l’entité cible quand ce n’est pas la police elle-même (ex. <see cref="Prime.Id"/>).</summary>
    public Guid? ReferenceId { get; set; }

    /// <summary>Nom logique du champ (ex. <c>ImportateurNom</c>, <c>ValeurFCFA</c>).</summary>
    [Required]
    [MaxLength(200)]
    public string NomChamp { get; set; } = string.Empty;

    [Column(TypeName = "nvarchar(max)")]
    public string? ValeurAvant { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string? ValeurApres { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string? Commentaire { get; set; }
}
