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
    [Table("Marchandises")]
    public class Marchandise : BaseModel
    {
        [ForeignKey("Assurance")]
        [Column("AssuranceId")]
        public Guid AssuranceId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Designation { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Nature { get; set; }

        [MaxLength(100)]
        public string? Specificites { get; set; }

        [Required]
        [MaxLength(500)]
        public string Conditionnement { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }
        public virtual Assurance? Assurance { get; set; }
        // Valeurs
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ValeurFCFA { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? ValeurDevise { get; set; }
        /// <summary>Code devise (ex: XOF, EUR). Colonne en base : Devise.</summary>
        [Column("Devise")]
        [MaxLength(50)]
        public string Devise { get; set; } = string.Empty;
        // Informations complémentaires
        [MaxLength(255)]
        public string? MasseBrute { get; set; }
        [MaxLength(255)]
        public string UniteStatistique { get; set; } = string.Empty;
        [MaxLength(255)]
        public string? Marque { get; set; }
    }
}
