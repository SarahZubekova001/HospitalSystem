using Hospital.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Api.Data;

public class HospitalDbContext : DbContext
{
    public HospitalDbContext(DbContextOptions<HospitalDbContext> options) : base(options) { }

    public DbSet<City> Cities => Set<City>();
    public DbSet<Person> Persons => Set<Person>();
    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Staff> Staff => Set<Staff>();
    public DbSet<InsuranceCompany> InsuranceCompanies => Set<InsuranceCompany>();
    public DbSet<Specialization> Specializations => Set<Specialization>();
    public DbSet<Diagnosis> Diagnoses => Set<Diagnosis>();
    public DbSet<Medication> Medications => Set<Medication>();
    public DbSet<AppointmentStatus> AppointmentStatuses => Set<AppointmentStatus>();
    public DbSet<AppointmentType> AppointmentTypes => Set<AppointmentType>();
    public DbSet<WorkingHours> WorkingHours => Set<WorkingHours>();
    public DbSet<AppointmentSlot> AppointmentSlots => Set<AppointmentSlot>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<MedicalRecord> MedicalRecords => Set<MedicalRecord>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserAccount>().HasIndex(x => x.PersonId).IsUnique();
        modelBuilder.Entity<UserAccount>().HasIndex(x => x.LoginEmail).IsUnique();
        modelBuilder.Entity<Role>().HasIndex(x => x.Name).IsUnique();
        modelBuilder.Entity<InsuranceCompany>().HasIndex(x => x.Code).IsUnique();
        modelBuilder.Entity<Specialization>().HasIndex(x => x.Name).IsUnique();
        modelBuilder.Entity<Diagnosis>().HasIndex(x => x.Icd10Code).IsUnique();
        modelBuilder.Entity<Staff>().HasIndex(x => x.LicenseNumber).IsUnique();
        modelBuilder.Entity<Patient>().HasIndex(x => x.BirthNumber).IsUnique();
        modelBuilder.Entity<MedicalRecord>().HasIndex(x => x.RecordNumber).IsUnique();

        modelBuilder.Entity<Appointment>()
            .HasIndex(x => x.AppointmentSlotId)
            .IsUnique();

        modelBuilder.Entity<MedicalRecord>()
            .HasIndex(x => x.AppointmentId)
            .IsUnique();

        modelBuilder.Entity<AppointmentSlot>()
            .HasOne(s => s.Appointment)
            .WithOne(a => a.AppointmentSlot)
            .HasForeignKey<Appointment>(a => a.AppointmentSlotId);

        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.MedicalRecord)
            .WithOne(m => m.Appointment)
            .HasForeignKey<MedicalRecord>(m => m.AppointmentId);

        modelBuilder.Entity<MedicalRecord>()
            .HasOne(m => m.Diagnosis)
            .WithMany(d => d.MedicalRecords)  
            .HasForeignKey(m => m.DiagnosisId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<MedicalRecord>()
            .HasIndex(m => m.DiagnosisId);

        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<City>().ToTable("City");
        modelBuilder.Entity<Person>().ToTable("Person");
        modelBuilder.Entity<Role>().ToTable("Role");
        modelBuilder.Entity<UserAccount>().ToTable("User_Account");
        modelBuilder.Entity<Patient>().ToTable("Patient");
        modelBuilder.Entity<Staff>().ToTable("Staff");
        modelBuilder.Entity<InsuranceCompany>().ToTable("Insurance_Company");
        modelBuilder.Entity<Specialization>().ToTable("Specialization");
        modelBuilder.Entity<Diagnosis>().ToTable("Diagnosis");
        modelBuilder.Entity<Medication>().ToTable("Medication");
        modelBuilder.Entity<AppointmentStatus>().ToTable("Appointment_Status");
        modelBuilder.Entity<AppointmentType>().ToTable("Appointment_Type");
        modelBuilder.Entity<WorkingHours>().ToTable("Working_Hours");
        modelBuilder.Entity<AppointmentSlot>().ToTable("Appointment_Slot");
        modelBuilder.Entity<Appointment>().ToTable("Appointment");
        modelBuilder.Entity<MedicalRecord>().ToTable("Medical_Record");
        modelBuilder.Entity<Prescription>().ToTable("Prescription");
    }
}
