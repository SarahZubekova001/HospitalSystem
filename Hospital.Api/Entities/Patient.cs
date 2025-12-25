using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital.Api.Entities;

public class Patient
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

    [Column("primary_doctor_id")]
    public int? PrimaryDoctorId { get; set; }

    [ForeignKey(nameof(PrimaryDoctorId))]
    public Staff? PrimaryDoctor { get; set; }

    [Column("blood_type")]
    [StringLength(5)]
    public string? BloodType { get; set; }

    [Required]
    [Column("insurance_company_id")]
    public int InsuranceCompanyId { get; set; }

    [ForeignKey(nameof(InsuranceCompanyId))]
    public InsuranceCompany InsuranceCompany { get; set; } = default!;

    [Required]
    [Column("is_active")]
    public bool IsActive { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
}
