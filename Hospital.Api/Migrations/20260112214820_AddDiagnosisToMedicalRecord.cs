using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hospital.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDiagnosisToMedicalRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "diagnosis_id",
                table: "Medical_Record",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Medical_Record_diagnosis_id",
                table: "Medical_Record",
                column: "diagnosis_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Medical_Record_Diagnosis_diagnosis_id",
                table: "Medical_Record",
                column: "diagnosis_id",
                principalTable: "Diagnosis",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Medical_Record_Diagnosis_diagnosis_id",
                table: "Medical_Record");

            migrationBuilder.DropIndex(
                name: "IX_Medical_Record_diagnosis_id",
                table: "Medical_Record");

            migrationBuilder.DropColumn(
                name: "diagnosis_id",
                table: "Medical_Record");
        }
    }
}
