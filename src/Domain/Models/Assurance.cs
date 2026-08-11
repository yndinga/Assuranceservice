using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AssuranceService.Domain.Models.Commons;

namespace AssuranceService.Domain.Models;

[Table("Assurances")]
public class Assurance : BaseModel
{
    [Required]
    [MaxLength(50)]
    public string NumeroAFI { get; set; } = string.Empty;

    public string? NoPolice { get; set; }

    [Column(TypeName = "nvarchar(250)")]
    [MaxLength(250)]
    public string? NumeroCert { get; set; }

    public string Etat { get; set; } = "42";

    [Column(TypeName = "nvarchar(250)")]
    public string? NoFacture { get; set; }

    [Column(TypeName = "nvarchar(250)")]
    [MaxLength(250)]
    public string? Partenaire { get; set; }

    [Column(TypeName = "nvarchar(250)")]
    [MaxLength(250)]
    public string? Intermediaire { get; set; }

    [Column(TypeName = "nvarchar(50)")]
    [MaxLength(50)]
    public string TypePartenaire { get; set; } = "ASSUREUR";

    [Column(TypeName = "nvarchar(250)")]
    public string ImportateurNom { get; set; } = string.Empty;

    [Column(TypeName = "nvarchar(250)")]
    public string ImportateurNIU { get; set; } = string.Empty;

    [Column(TypeName = "datetime2")]
    public DateTime? DateDebut { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime? DateFin { get; set; }

    [Column(TypeName = "nvarchar(250)")]
    public string TypeContrat { get; set; } = string.Empty;

    [Column(TypeName = "nvarchar(250)")]
    public string? Duree { get; set; }

    /// <summary>
    /// Durée contractuelle normalisée. Le champ texte Duree est conservé
    /// temporairement pour la compatibilité des anciens clients et dossiers.
    /// </summary>
    public int? DureeJours { get; set; }

    public Guid? GarantieId { get; set; }

    public string OCRE { get; set; } = string.Empty;
    public string PCRE { get; set; } = string.Empty;

    [MaxLength(10)]
    public string ModeDeTransport { get; set; } = string.Empty;

    public virtual Garantie? Garantie { get; set; }
    public virtual Voyage? Voyage { get; set; }
    public virtual ICollection<Prime> Primes { get; set; } = new List<Prime>();
    public virtual ICollection<VisaAssurance> Visas { get; set; } = new List<VisaAssurance>();
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
    public virtual ICollection<AssuranceDeclaration> Declarations { get; set; } = new List<AssuranceDeclaration>();
    public virtual ICollection<AssuranceLigne> Lignes { get; set; } = new List<AssuranceLigne>();

    [NotMapped]
    public decimal ValeurDeviseTotale => Lignes.Sum(ligne => ligne.ValeurDevise ?? 0m);

    [NotMapped]
    public decimal ValeurXAFTotale => Lignes.Sum(ligne => ligne.ValeurXAF ?? 0m);

    [NotMapped]
    public decimal MasseBruteTotale => Lignes.Sum(ligne => ligne.MasseBrute ?? 0m);

    [NotMapped]
    public decimal MasseNetteTotale => Lignes.Sum(ligne => ligne.MasseNette ?? 0m);

    [NotMapped]
    public decimal VolumeTotal => Lignes.Sum(ligne => ligne.Volume ?? 0m);

    public static string GenererNumeroAFI(Guid assuranceId) =>
        $"AFI-{assuranceId:N}".ToUpperInvariant();
}
