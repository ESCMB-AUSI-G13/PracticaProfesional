using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PracticaProfesional.Migrations
{
    /// <inheritdoc />
    public partial class AddCarrera : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Crear tabla Carreras primero
            migrationBuilder.CreateTable(
                name: "Carreras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Resolucion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carreras", x => x.Id);
                });

            // 2. Sembrar las 2 carreras ANTES de agregar las FKs
            migrationBuilder.Sql("INSERT INTO Carreras (Nombre, Resolucion) VALUES (N'Profesorado de Educación Secundaria en Economía', N'Res. 0013')");
            migrationBuilder.Sql("INSERT INTO Carreras (Nombre, Resolucion) VALUES (N'Trayecto Pedagógico para Graduados No Docentes', N'Res. 104/22')");

            // 3. Eliminar columna Plan de Materias
            migrationBuilder.DropColumn(
                name: "Plan",
                table: "Materias");

            // 4. Agregar CarreraId a Materias y Estudiantes (default 0 temporalmente)
            migrationBuilder.AddColumn<int>(
                name: "CarreraId",
                table: "Materias",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CarreraId",
                table: "Estudiantes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // 5. Apuntar todos los registros existentes a la carrera 1 (Profesorado)
            migrationBuilder.Sql("UPDATE Materias SET CarreraId = 1 WHERE CarreraId = 0");
            migrationBuilder.Sql("UPDATE Estudiantes SET CarreraId = 1 WHERE CarreraId = 0");

            // 6. Crear índices
            migrationBuilder.CreateIndex(
                name: "IX_Materias_CarreraId",
                table: "Materias",
                column: "CarreraId");

            migrationBuilder.CreateIndex(
                name: "IX_Estudiantes_CarreraId",
                table: "Estudiantes",
                column: "CarreraId");

            migrationBuilder.CreateIndex(
                name: "IX_Carreras_Nombre",
                table: "Carreras",
                column: "Nombre",
                unique: true);

            // 7. Agregar FKs ahora que todos los datos son válidos
            migrationBuilder.AddForeignKey(
                name: "FK_Estudiantes_Carreras_CarreraId",
                table: "Estudiantes",
                column: "CarreraId",
                principalTable: "Carreras",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Materias_Carreras_CarreraId",
                table: "Materias",
                column: "CarreraId",
                principalTable: "Carreras",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Estudiantes_Carreras_CarreraId",
                table: "Estudiantes");

            migrationBuilder.DropForeignKey(
                name: "FK_Materias_Carreras_CarreraId",
                table: "Materias");

            migrationBuilder.DropTable(
                name: "Carreras");

            migrationBuilder.DropIndex(
                name: "IX_Materias_CarreraId",
                table: "Materias");

            migrationBuilder.DropIndex(
                name: "IX_Estudiantes_CarreraId",
                table: "Estudiantes");

            migrationBuilder.DropColumn(
                name: "CarreraId",
                table: "Materias");

            migrationBuilder.DropColumn(
                name: "CarreraId",
                table: "Estudiantes");

            migrationBuilder.AddColumn<string>(
                name: "Plan",
                table: "Materias",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }
    }
}
