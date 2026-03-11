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
    public class Prime : BaseModel
    {
        [Column(TypeName = "decimal(18,4)")]
        public decimal? Taux { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ValeurFCFA { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ValeurDevise { get; set; }

        public decimal? PrimeNette { get; set; }

        public double? Accessoires { get; set; }

        public decimal? Taxe { get; set; }

        public decimal? PrimeTotale { get; set; }

        [Required]
        [Column("AssuranceId")]
        [ForeignKey("Assurances")]
        public Guid AssuranceId { get; set; }

        [Required]
        [Column("Statut")]
        public string Statut { get; set; } = string.Empty;

    }
}
