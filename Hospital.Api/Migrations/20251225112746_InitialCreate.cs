using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hospital.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Appointment_Status",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointment_Status", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Appointment_Type",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    default_duration_minutes = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointment_Type", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    postal_code = table.Column<string>(type: "char(5)", maxLength: 5, nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.postal_code);
                });

            migrationBuilder.CreateTable(
                name: "Diagnoses",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    icd10_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Diagnoses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Insurance_Company",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Insurance_Company", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Medications",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    active_substance = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Specializations",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Specializations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Persons",
                columns: table => new
                {
                    birth_number = table.Column<string>(type: "char(11)", maxLength: 11, nullable: false),
                    first_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    street_address = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    city_postal_code = table.Column<string>(type: "char(5)", maxLength: 5, nullable: true),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Persons", x => x.birth_number);
                    table.ForeignKey(
                        name: "FK_Persons_Cities_city_postal_code",
                        column: x => x.city_postal_code,
                        principalTable: "Cities",
                        principalColumn: "postal_code");
                });

            migrationBuilder.CreateTable(
                name: "Staff",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    birth_number = table.Column<string>(type: "char(11)", maxLength: 11, nullable: false),
                    specialization_id = table.Column<int>(type: "integer", nullable: true),
                    license_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    work_phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Staff", x => x.id);
                    table.ForeignKey(
                        name: "FK_Staff_Persons_birth_number",
                        column: x => x.birth_number,
                        principalTable: "Persons",
                        principalColumn: "birth_number",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Staff_Specializations_specialization_id",
                        column: x => x.specialization_id,
                        principalTable: "Specializations",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "User_Account",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    person_id = table.Column<string>(type: "char(11)", maxLength: 11, nullable: false),
                    login_email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    role_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User_Account", x => x.id);
                    table.ForeignKey(
                        name: "FK_User_Account_Persons_person_id",
                        column: x => x.person_id,
                        principalTable: "Persons",
                        principalColumn: "birth_number",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_User_Account_Roles_role_id",
                        column: x => x.role_id,
                        principalTable: "Roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Appointment_Slot",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    staff_id = table.Column<int>(type: "integer", nullable: false),
                    start_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_available = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointment_Slot", x => x.id);
                    table.ForeignKey(
                        name: "FK_Appointment_Slot_Staff_staff_id",
                        column: x => x.staff_id,
                        principalTable: "Staff",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Patients",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    birth_number = table.Column<string>(type: "char(11)", maxLength: 11, nullable: false),
                    primary_doctor_id = table.Column<int>(type: "integer", nullable: true),
                    blood_type = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    insurance_company_id = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patients", x => x.id);
                    table.ForeignKey(
                        name: "FK_Patients_Insurance_Company_insurance_company_id",
                        column: x => x.insurance_company_id,
                        principalTable: "Insurance_Company",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Patients_Persons_birth_number",
                        column: x => x.birth_number,
                        principalTable: "Persons",
                        principalColumn: "birth_number",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Patients_Staff_primary_doctor_id",
                        column: x => x.primary_doctor_id,
                        principalTable: "Staff",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "Working_Hours",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    staff_id = table.Column<int>(type: "integer", nullable: false),
                    day_of_week = table.Column<int>(type: "integer", nullable: false),
                    start_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    end_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Working_Hours", x => x.id);
                    table.ForeignKey(
                        name: "FK_Working_Hours_Staff_staff_id",
                        column: x => x.staff_id,
                        principalTable: "Staff",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    appointment_slot_id = table.Column<int>(type: "integer", nullable: false),
                    patient_id = table.Column<int>(type: "integer", nullable: false),
                    appointment_type_id = table.Column<int>(type: "integer", nullable: false),
                    status_id = table.Column<int>(type: "integer", nullable: false),
                    reason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    notes = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.id);
                    table.ForeignKey(
                        name: "FK_Appointments_Appointment_Slot_appointment_slot_id",
                        column: x => x.appointment_slot_id,
                        principalTable: "Appointment_Slot",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Appointments_Appointment_Status_status_id",
                        column: x => x.status_id,
                        principalTable: "Appointment_Status",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Appointments_Appointment_Type_appointment_type_id",
                        column: x => x.appointment_type_id,
                        principalTable: "Appointment_Type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Appointments_Patients_patient_id",
                        column: x => x.patient_id,
                        principalTable: "Patients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Medical_Record",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    appointment_id = table.Column<int>(type: "integer", nullable: false),
                    patient_id = table.Column<int>(type: "integer", nullable: false),
                    staff_id = table.Column<int>(type: "integer", nullable: false),
                    record_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    results = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medical_Record", x => x.id);
                    table.ForeignKey(
                        name: "FK_Medical_Record_Appointments_appointment_id",
                        column: x => x.appointment_id,
                        principalTable: "Appointments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Medical_Record_Patients_patient_id",
                        column: x => x.patient_id,
                        principalTable: "Patients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Medical_Record_Staff_staff_id",
                        column: x => x.staff_id,
                        principalTable: "Staff",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Prescriptions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    medical_record_id = table.Column<int>(type: "integer", nullable: false),
                    medication_id = table.Column<int>(type: "integer", nullable: false),
                    dosage = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    valid_until = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prescriptions", x => x.id);
                    table.ForeignKey(
                        name: "FK_Prescriptions_Medical_Record_medical_record_id",
                        column: x => x.medical_record_id,
                        principalTable: "Medical_Record",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Prescriptions_Medications_medication_id",
                        column: x => x.medication_id,
                        principalTable: "Medications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Appointment_Slot_staff_id",
                table: "Appointment_Slot",
                column: "staff_id");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_appointment_slot_id",
                table: "Appointments",
                column: "appointment_slot_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_appointment_type_id",
                table: "Appointments",
                column: "appointment_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_patient_id",
                table: "Appointments",
                column: "patient_id");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_status_id",
                table: "Appointments",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "IX_Diagnoses_icd10_code",
                table: "Diagnoses",
                column: "icd10_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Insurance_Company_code",
                table: "Insurance_Company",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Medical_Record_appointment_id",
                table: "Medical_Record",
                column: "appointment_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Medical_Record_patient_id",
                table: "Medical_Record",
                column: "patient_id");

            migrationBuilder.CreateIndex(
                name: "IX_Medical_Record_record_number",
                table: "Medical_Record",
                column: "record_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Medical_Record_staff_id",
                table: "Medical_Record",
                column: "staff_id");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_birth_number",
                table: "Patients",
                column: "birth_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Patients_insurance_company_id",
                table: "Patients",
                column: "insurance_company_id");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_primary_doctor_id",
                table: "Patients",
                column: "primary_doctor_id");

            migrationBuilder.CreateIndex(
                name: "IX_Persons_city_postal_code",
                table: "Persons",
                column: "city_postal_code");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_medical_record_id",
                table: "Prescriptions",
                column: "medical_record_id");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_medication_id",
                table: "Prescriptions",
                column: "medication_id");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_name",
                table: "Roles",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Specializations_name",
                table: "Specializations",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Staff_birth_number",
                table: "Staff",
                column: "birth_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Staff_license_number",
                table: "Staff",
                column: "license_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Staff_specialization_id",
                table: "Staff",
                column: "specialization_id");

            migrationBuilder.CreateIndex(
                name: "IX_User_Account_login_email",
                table: "User_Account",
                column: "login_email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_Account_person_id",
                table: "User_Account",
                column: "person_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_Account_role_id",
                table: "User_Account",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_Working_Hours_staff_id",
                table: "Working_Hours",
                column: "staff_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Diagnoses");

            migrationBuilder.DropTable(
                name: "Prescriptions");

            migrationBuilder.DropTable(
                name: "User_Account");

            migrationBuilder.DropTable(
                name: "Working_Hours");

            migrationBuilder.DropTable(
                name: "Medical_Record");

            migrationBuilder.DropTable(
                name: "Medications");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Appointments");

            migrationBuilder.DropTable(
                name: "Appointment_Slot");

            migrationBuilder.DropTable(
                name: "Appointment_Status");

            migrationBuilder.DropTable(
                name: "Appointment_Type");

            migrationBuilder.DropTable(
                name: "Patients");

            migrationBuilder.DropTable(
                name: "Insurance_Company");

            migrationBuilder.DropTable(
                name: "Staff");

            migrationBuilder.DropTable(
                name: "Persons");

            migrationBuilder.DropTable(
                name: "Specializations");

            migrationBuilder.DropTable(
                name: "Cities");
        }
    }
}
