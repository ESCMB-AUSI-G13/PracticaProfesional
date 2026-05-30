using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PracticaProfesional.Migrations
{
    /// <inheritdoc />
    public partial class EnsureFechaDeEgresoEstudiante : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Agrega la columna solo si no existe (idempotente).
            // Necesario porque la migración anterior puede haber quedado
            // registrada en __EFMigrationsHistory sin ejecutar el DDL.
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE Name = N'FechaDeEgreso'
                      AND Object_ID = Object_ID(N'Estudiantes'))
                BEGIN
                    ALTER TABLE [Estudiantes] ADD [FechaDeEgreso] datetime2 NULL;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE Name = N'FechaDeEgreso'
                      AND Object_ID = Object_ID(N'Estudiantes'))
                BEGIN
                    ALTER TABLE [Estudiantes] DROP COLUMN [FechaDeEgreso];
                END
            ");
        }
    }
}
