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
    public class Garantie: BaseModel
    {
        [Required]
        [MaxLength(255)]
        public string Nom { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,4)")]
        public decimal? Taux { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Accessoires { get; set; }

        [Required]
        public bool Actif { get; set; } = true;
    }
}
