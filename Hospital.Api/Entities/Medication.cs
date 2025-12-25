using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital.Api.Entities;

public class Medication
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("name")]
    [StringLength(50)]
    public string Name { get; set; } = default!;

    [Column("active_substance")]
    [StringLength(100)]
    public string? ActiveSubstance { get; set; }

    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
}
