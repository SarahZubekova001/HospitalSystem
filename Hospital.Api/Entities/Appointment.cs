using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital.Api.Entities;

public class Appointment
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("appointment_slot_id")]
    public int AppointmentSlotId { get; set; }

    [ForeignKey(nameof(AppointmentSlotId))]
    public AppointmentSlot AppointmentSlot { get; set; } = default!;

    [Required]
    [Column("patient_id")]
    public int PatientId { get; set; }

    [ForeignKey(nameof(PatientId))]
    public Patient Patient { get; set; } = default!;

    [Required]
    [Column("appointment_type_id")]
    public int AppointmentTypeId { get; set; }

    [ForeignKey(nameof(AppointmentTypeId))]
    public AppointmentType AppointmentType { get; set; } = default!;

    [Required]
    [Column("status_id")]
    public int StatusId { get; set; }

    [ForeignKey(nameof(StatusId))]
    public AppointmentStatus Status { get; set; } = default!;

    [Column("reason")]
    [StringLength(200)]
    public string? Reason { get; set; }

    [Column("notes")]
    [StringLength(200)]
    public string? Notes { get; set; }

    public MedicalRecord? MedicalRecord { get; set; }
}
