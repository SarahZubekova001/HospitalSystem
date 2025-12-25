using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital.Api.Entities;

public class Prescription
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("medical_record_id")]
    public int MedicalRecordId { get; set; }

    [ForeignKey(nameof(MedicalRecordId))]
    public MedicalRecord MedicalRecord { get; set; } = default!;

    [Required]
    [Column("medication_id")]
    public int MedicationId { get; set; }

    [ForeignKey(nameof(MedicationId))]
    public Medication Medication { get; set; } = default!;

    [Required]
    [Column("dosage")]
    [StringLength(100)]
    public string Dosage { get; set; } = default!;

    [Required]
    [Column("quantity")]
    public int Quantity { get; set; }

    [Column("valid_until")]
    public DateOnly? ValidUntil { get; set; }
}
