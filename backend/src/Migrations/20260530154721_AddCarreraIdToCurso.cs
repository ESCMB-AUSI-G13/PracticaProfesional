using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PracticaProfesional.Migrations
{
    /// <inheritdoc />
    public partial class AddCarreraIdToCurso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Cursos_Anio_AnioLectivo_Comision",
                table: "Cursos");

            migrationBuilder.AddColumn<int>(
                name: "CarreraId",
                table: "Cursos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Cursos_Anio_AnioLectivo_Comision_CarreraId",
                table: "Cursos",
                columns: new[] { "Anio", "AnioLectivo", "Comision", "CarreraId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Cursos_Anio_AnioLectivo_Comision_CarreraId",
                table: "Cursos");

            migrationBuilder.DropColumn(
                name: "CarreraId",
                table: "Cursos");

            migrationBuilder.CreateIndex(
                name: "IX_Cursos_Anio_AnioLectivo_Comision",
                table: "Cursos",
                columns: new[] { "Anio", "AnioLectivo", "Comision" },
                unique: true);
        }
    }
}
