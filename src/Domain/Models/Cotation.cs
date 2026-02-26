using AssuranceService.Domain.Models.Commons;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssuranceService.Domain.Models
{
    public class Cotation: BaseModel
    {
        
        [Required]
        public double Montant { get; set; }

        [MaxLength(255)]
        public string? Taux { get; set; }

        public double? PrimeNette { get; set; }

        public double? Accessoires { get; set; }

        public double? Taxe { get; set; }

        public double? PrimeTotale { get; set; }

        [Required]
        [ForeignKey("Contrat")]
        public ulong ContratId { get; set; }

        [Required]
        [ForeignKey("Partenaire")]
        public ulong PartenaireId { get; set; }

        [Required]
        [ForeignKey("User")]
        public ulong UserId { get; set; }

        [Required]
        [ForeignKey("Statut")]
        public ulong StatutId { get; set; }
    }
}
