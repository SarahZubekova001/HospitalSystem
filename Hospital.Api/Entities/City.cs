using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital.Api.Entities;

public class City
{
    [Key]
    [Column("postal_code", TypeName = "char(5)")]
    [StringLength(5)]
    public string PostalCode { get; set; } = default!;

    [Required]
    [Column("name")]
    [StringLength(50)]
    public string Name { get; set; } = default!;

    public ICollection<Person> Persons { get; set; } = new List<Person>();
}
