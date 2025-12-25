using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital.Api.Entities;

public class Staff
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("birth_number", TypeName = "char(11)")]
    [StringLength(11)]
    public string BirthNumber { get; set; } = default!;

    [ForeignKey(nameof(BirthNumber))]
    public Person Person { get; set; } = default!;

    [Column("specialization_id")]
    public int? SpecializationId { get; set; }

    [ForeignKey(nameof(SpecializationId))]
    public Specialization? Specialization { get; set; }

    [Required]
    [Column("license_number")]
    [StringLength(50)]
    public string LicenseNumber { get; set; } = default!;

    [Column("work_phone")]
    [StringLength(20)]
    public string? WorkPhone { get; set; }

    public ICollection<WorkingHours> WorkingHours { get; set; } = new List<WorkingHours>();
    public ICollection<AppointmentSlot> AppointmentSlots { get; set; } = new List<AppointmentSlot>();
}
