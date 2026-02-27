using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AssuranceService.Domain.Models.Commons;

public abstract class BaseModel
{
    [Key]
    [Column("Id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [NotMapped]
    [JsonIgnore]
    public Guid ID { get => Id; set => Id = value; }

    public string? CreerPar { get; set; }
    public string? ModifierPar { get; set; }
    public DateTime CreerLe { get; set; }
    public DateTime? ModifierLe { get; set; }
}
