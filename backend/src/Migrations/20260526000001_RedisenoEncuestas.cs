using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PracticaProfesional.Migrations
{
    /// <inheritdoc />
    public partial class RedisenoEncuestas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── Limpiar tabla Encuestas existente (feature estaba Pendiente, sin datos) ──
            migrationBuilder.Sql("DELETE FROM RespuestasEncuesta");
            migrationBuilder.Sql("DELETE FROM Encuestas");

            // ── Modificar Encuestas ───────────────────────────────────────────────────
            migrationBuilder.DropForeignKey(
                name: "FK_Encuestas_Docentes_DocenteId",
                table: "Encuestas");

            migrationBuilder.DropForeignKey(
                name: "FK_Encuestas_Materias_MateriaId",
                table: "Encuestas");

            migrationBuilder.DropIndex(
                name: "IX_Encuestas_DocenteId",
                table: "Encuestas");

            migrationBuilder.DropIndex(
                name: "IX_Encuestas_MateriaId",
                table: "Encuestas");

            migrationBuilder.DropColumn(name: "DocenteId",  table: "Encuestas");
            migrationBuilder.DropColumn(name: "Preguntas",  table: "Encuestas");

            migrationBuilder.AddColumn<string>(
                name: "Titulo",
                table: "Encuestas",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Descripcion",
                table: "Encuestas",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tipo",
                table: "Encuestas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "SatisfaccionGeneral");

            migrationBuilder.AddColumn<int>(
                name: "CicloLectivo",
                table: "Encuestas",
                type: "int",
                nullable: false,
                defaultValue: 2026);

            // MateriaId ahora es nullable
            migrationBuilder.AlterColumn<int>(
                name: "MateriaId",
                table: "Encuestas",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: false);

            migrationBuilder.AddForeignKey(
                name: "FK_Encuestas_Materias_MateriaId",
                table: "Encuestas",
                column: "MateriaId",
                principalTable: "Materias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.CreateIndex(
                name: "IX_Encuestas_MateriaId",
                table: "Encuestas",
                column: "MateriaId");

            // ── Modificar RespuestasEncuesta ─────────────────────────────────────────
            migrationBuilder.DropColumn(name: "Preguntas",  table: "RespuestasEncuesta");
            migrationBuilder.DropColumn(name: "Respuestas", table: "RespuestasEncuesta");

            // ── Crear PreguntasEncuesta ───────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "PreguntasEncuesta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EncuestaId    = table.Column<int>(type: "int", nullable: false),
                    Texto         = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Orden         = table.Column<int>(type: "int", nullable: false),
                    TipoPregunta  = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EsObligatoria = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreguntasEncuesta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreguntasEncuesta_Encuestas_EncuestaId",
                        column: x => x.EncuestaId,
                        principalTable: "Encuestas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PreguntasEncuesta_EncuestaId",
                table: "PreguntasEncuesta",
                column: "EncuestaId");

            // ── Crear ItemsRespuesta ──────────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "ItemsRespuesta",
                columns: table => new
                {
                    Id                  = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RespuestaEncuestaId = table.Column<int>(type: "int", nullable: false),
                    PreguntaId          = table.Column<int>(type: "int", nullable: false),
                    ValorNumerico       = table.Column<int>(type: "int", nullable: true),
                    TextoLibre          = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemsRespuesta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemsRespuesta_RespuestasEncuesta_RespuestaEncuestaId",
                        column: x => x.RespuestaEncuestaId,
                        principalTable: "RespuestasEncuesta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemsRespuesta_PreguntasEncuesta_PreguntaId",
                        column: x => x.PreguntaId,
                        principalTable: "PreguntasEncuesta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemsRespuesta_RespuestaEncuestaId",
                table: "ItemsRespuesta",
                column: "RespuestaEncuestaId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemsRespuesta_PreguntaId",
                table: "ItemsRespuesta",
                column: "PreguntaId");

            // ── Crear EncuestasCompletadas ────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "EncuestasCompletadas",
                columns: table => new
                {
                    Id              = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TokenAnonimo    = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    EncuestaId      = table.Column<int>(type: "int", nullable: false),
                    FechaCompletada = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EncuestasCompletadas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EncuestasCompletadas_Encuestas_EncuestaId",
                        column: x => x.EncuestaId,
                        principalTable: "Encuestas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EncuestasCompletadas_TokenAnonimo_EncuestaId",
                table: "EncuestasCompletadas",
                columns: new[] { "TokenAnonimo", "EncuestaId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "EncuestasCompletadas");
            migrationBuilder.DropTable(name: "ItemsRespuesta");
            migrationBuilder.DropTable(name: "PreguntasEncuesta");

            migrationBuilder.AddColumn<string>(
                name: "Preguntas",
                table: "RespuestasEncuesta",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Respuestas",
                table: "RespuestasEncuesta",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.DropForeignKey(name: "FK_Encuestas_Materias_MateriaId", table: "Encuestas");
            migrationBuilder.DropIndex(name: "IX_Encuestas_MateriaId", table: "Encuestas");
            migrationBuilder.DropColumn(name: "Titulo",       table: "Encuestas");
            migrationBuilder.DropColumn(name: "Descripcion",  table: "Encuestas");
            migrationBuilder.DropColumn(name: "Tipo",         table: "Encuestas");
            migrationBuilder.DropColumn(name: "CicloLectivo", table: "Encuestas");

            migrationBuilder.AddColumn<int>(
                name: "DocenteId",
                table: "Encuestas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Preguntas",
                table: "Encuestas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "MateriaId",
                table: "Encuestas",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
