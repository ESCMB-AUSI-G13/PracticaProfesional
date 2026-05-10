using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PracticaProfesional.Migrations;

/// <inheritdoc />
public partial class FixCursosUniqueConstraint : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // El schema.sql original creó UNIQUE (Anio, Comision) sin AnioLectivo.
        // El modelo EF ya tiene el índice correcto: (Anio, AnioLectivo, Comision).
        // Se elimina el constraint incorrecto para permitir cursos del mismo año
        // calendario en distintos años lectivos (1°, 2°, 3°, 4°).
        migrationBuilder.Sql(
            "IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'UQ_Cursos_AnioCom') " +
            "ALTER TABLE [Cursos] DROP CONSTRAINT [UQ_Cursos_AnioCom]"
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddUniqueConstraint(
            name: "UQ_Cursos_AnioCom",
            table: "Cursos",
            columns: new[] { "Anio", "Comision" });
    }
}
