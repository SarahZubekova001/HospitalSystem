using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital.Api.Entities;

[Table("Appointment_Slot")]
public class AppointmentSlot
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("staff_id")]
    public int StaffId { get; set; }

    [ForeignKey(nameof(StaffId))]
    public Staff Staff { get; set; } = default!;

    [Required]
    [Column("start_time")]
    public DateTime StartTime { get; set; }

    [Required]
    [Column("end_time")]
    public DateTime EndTime { get; set; }

    [Required]
    [Column("is_available")]
    public bool IsAvailable { get; set; } = true;

    public Appointment? Appointment { get; set; }
}
