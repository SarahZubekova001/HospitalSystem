using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital.Api.Entities;

[Table("Medical_Record")]
public class MedicalRecord
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("appointment_id")]
    public int AppointmentId { get; set; }

    [ForeignKey(nameof(AppointmentId))]
    public Appointment Appointment { get; set; } = default!;

    [Required]
    [Column("patient_id")]
    public int PatientId { get; set; }

    [ForeignKey(nameof(PatientId))]
    public Patient Patient { get; set; } = default!;

    [Required]
    [Column("staff_id")]
    public int StaffId { get; set; }

    [ForeignKey(nameof(StaffId))]
    public Staff Staff { get; set; } = default!;

    [Required]
    [Column("record_number")]
    [StringLength(50)]
    public string RecordNumber { get; set; } = default!;

    [Column("diagnosis_id")]
    public int? DiagnosisId { get; set; }

    [ForeignKey(nameof(DiagnosisId))]
    public Diagnosis? Diagnosis { get; set; }

    [Column("results")]
    [StringLength(4000)]
    public string? Results { get; set; }

    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
}
