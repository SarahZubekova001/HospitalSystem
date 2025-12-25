using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital.Api.Entities;

[Table("Insurance_Company")]
public class InsuranceCompany
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("name")]
    [StringLength(50)]
    public string Name { get; set; } = default!;

    [Required]
    [Column("code")]
    [StringLength(10)]
    public string Code { get; set; } = default!;

    public ICollection<Patient> Patients { get; set; } = new List<Patient>();
}
