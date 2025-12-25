using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital.Api.Entities;

[Table("Working_Hours")]
public class WorkingHours
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
    [Column("day_of_week")]
    public int DayOfWeek { get; set; }

    [Required]
    [Column("start_time")]
    public TimeOnly StartTime { get; set; }

    [Required]
    [Column("end_time")]
    public TimeOnly EndTime { get; set; }
}
