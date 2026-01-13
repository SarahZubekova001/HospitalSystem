using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital.Api.Entities;

[Table("Diagnosis")]
public class Diagnosis
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("icd10_code")]
    [StringLength(10)]
    public string Icd10Code { get; set; } = default!;

    [Required]
    [Column("name")]
    [StringLength(255)]
    public string Name { get; set; } = default!;

    [Column("description")]
    [StringLength(200)]
    public string? Description { get; set; }

    public ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
}
