using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PracticaProfesional.Migrations
{
    /// <inheritdoc />
    public partial class AgregarPadronAlumno : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PadronAlumnos",
                columns: table => new
                {
                    DNI = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    FechaCarga = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PadronAlumnos", x => x.DNI);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PadronAlumnos");
        }
    }
}
