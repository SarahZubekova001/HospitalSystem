using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital.Api.Entities;

[Table("Appointment_Type")]
public class AppointmentType
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("name")]
    [StringLength(50)]
    public string Name { get; set; } = default!;

    [Column("description")]
    [StringLength(200)]
    public string? Description { get; set; }

    [Required]
    [Column("default_duration_minutes")]
    public int DefaultDurationMinutes { get; set; }
}
