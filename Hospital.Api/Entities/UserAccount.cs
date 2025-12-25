using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital.Api.Entities;

[Table("User_Account")]
public class UserAccount
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("person_id", TypeName = "char(11)")]
    [StringLength(11)]
    public string PersonId { get; set; } = default!;

    [ForeignKey(nameof(PersonId))]
    public Person Person { get; set; } = default!;

    [Required]
    [Column("login_email")]
    [StringLength(255)]
    public string LoginEmail { get; set; } = default!;

    [Required]
    [Column("password_hash")]
    [StringLength(255)]
    public string PasswordHash { get; set; } = default!;

    [Required]
    [Column("role_id")]
    public int RoleId { get; set; }

    [ForeignKey(nameof(RoleId))]
    public Role Role { get; set; } = default!;
}
