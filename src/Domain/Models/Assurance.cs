using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssuranceService.Domain.Models.Commons;

namespace AssuranceService.Domain.Models
{
    [Table("Assurances")]
    public class Assurance : BaseModel
    {
        public string? NoPolice { get; set; }
         [Column(TypeName = "nvarchar(250)")] 
        [MaxLength(250)]
        public string? NumeroCert { get; set; }
        [Column(TypeName = "nvarchar(250)")] 
        public string ImportateurNom { get; set; } = string.Empty;

        [Column(TypeName = "nvarchar(250)")] 
        public string ImportateurNIU { get; set; } = string.Empty;

        [Column(TypeName = "datetime2")]
        public DateTime? DateDebut { get; set; }
        [Column(TypeName = "datetime2")]
        public DateTime? DateFin { get; set; }
        // Contrat
        [Column(TypeName = "nvarchar(250)")] 
        public string TypeContrat { get; set; } = string.Empty;

        [Column(TypeName = "nvarchar(250)")] 
        public string? Duree { get; set; }

        /// <summary>Code statut (référentiel Statuts). Défaut : 10 = Elaboré.</summary>
        [Column(TypeName = "nvarchar(10)")]
        [MaxLength(10)]
        public string Statut { get; set; } = "10";

        [Column(TypeName = "nvarchar(250)")]
        public string Module { get; set; } = string.Empty;

        public Guid? GarantieId { get; set; }
        public virtual Garantie? Garantie { get; set; }
        public Guid? AssureurId { get; set; }
        public Guid? IntermediaireId { get; set; }
        public string OCRE { get; set; } = string.Empty;

        // Données cargaison (fusionnées dans Assurances)
        [Column(TypeName = "nvarchar(255)")]
        public string? Designation { get; set; }
        [Column(TypeName = "nvarchar(500)")]
        public string? Nature { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        public string? Specificites { get; set; }
        [Column(TypeName = "nvarchar(500)")]
        public string? Conditionnement { get; set; }
        [Column(TypeName = "nvarchar(500)")]
        public string? Description { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public string? Devise { get; set; }
        [Column(TypeName = "nvarchar(255)")]
        public string? MasseBrute { get; set; }
        [Column(TypeName = "nvarchar(255)")]
        public string? UniteStatistique { get; set; }
        [Column(TypeName = "nvarchar(255)")]
        public string? Marque { get; set; }

        // Transport (fusionné dans Assurances)
        [Column(TypeName = "nvarchar(255)")]
        public string? NomTransporteur { get; set; }
        [Column(TypeName = "nvarchar(255)")]
        public string? NomNavire { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        public string? TypeNavire { get; set; }
        [Column(TypeName = "nvarchar(255)")]
        public string? LieuSejour { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public string? DureeSejour { get; set; }
        [Column(TypeName = "nvarchar(255)")]
        public string? PaysProvenance { get; set; }
        [Column(TypeName = "nvarchar(255)")]
        public string? PaysDestination { get; set; }
        public virtual ICollection<Prime> Primes { get; set; } = new List<Prime>();
        public virtual ICollection<VisaAssurance> Visas { get; set; } = new List<VisaAssurance>();
        public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
        public virtual Maritime? Maritime { get; set; }
        public virtual Aerien? Aerien { get; set; }
        public virtual Routier? Routier { get; set; }
        public virtual Fluvial? Fluvial { get; set; }
    }
}



