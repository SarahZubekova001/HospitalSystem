using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital.Api.Entities;

public class Person
{
    [Key]
    [Column("birth_number", TypeName = "char(11)")]
    [StringLength(11)]
    public string BirthNumber { get; set; } = default!;

    [Required]
    [Column("first_name")]
    [StringLength(50)]
    public string FirstName { get; set; } = default!;

    [Required]
    [Column("last_name")]
    [StringLength(50)]
    public string LastName { get; set; } = default!;

    [Column("street_address")]
    [StringLength(255)]
    public string? StreetAddress { get; set; }

    [Column("city_postal_code", TypeName = "char(5)")]
    [StringLength(5)]
    public string? CityPostalCode { get; set; }

    [ForeignKey(nameof(CityPostalCode))]
    public City? City { get; set; }

    [Column("phone")]
    [StringLength(20)]
    public string? Phone { get; set; }

    [Column("email")]
    [StringLength(255)]
    public string? Email { get; set; }

    public UserAccount? UserAccount { get; set; }
    public Patient? Patient { get; set; }
    public Staff? Staff { get; set; }
}
