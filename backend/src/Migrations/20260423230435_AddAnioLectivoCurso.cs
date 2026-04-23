using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PracticaProfesional.Migrations
{
    /// <inheritdoc />
    public partial class AddAnioLectivoCurso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PasswordResetToken",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Cursos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Anio = table.Column<int>(type: "int", nullable: false),
                    AnioLectivo = table.Column<int>(type: "int", nullable: false),
                    Comision = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Cupo = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PreceptorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cursos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cursos_Preceptores_PreceptorId",
                        column: x => x.PreceptorId,
                        principalTable: "Preceptores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Materias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Plan = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Materias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Asistencias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EstudianteId = table.Column<int>(type: "int", nullable: false),
                    MateriaId = table.Column<int>(type: "int", nullable: false),
                    CursoId = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asistencias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Asistencias_Cursos_CursoId",
                        column: x => x.CursoId,
                        principalTable: "Cursos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Asistencias_Estudiantes_EstudianteId",
                        column: x => x.EstudianteId,
                        principalTable: "Estudiantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Asistencias_Materias_MateriaId",
                        column: x => x.MateriaId,
                        principalTable: "Materias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CalendarioAcademico",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreEvento = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Comision = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TipoEvento = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MateriaId = table.Column<int>(type: "int", nullable: true),
                    CursoId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarioAcademico", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalendarioAcademico_Cursos_CursoId",
                        column: x => x.CursoId,
                        principalTable: "Cursos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CalendarioAcademico_Materias_MateriaId",
                        column: x => x.MateriaId,
                        principalTable: "Materias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Correlatividades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MateriaDestinoId = table.Column<int>(type: "int", nullable: false),
                    MateriaRequisitoId = table.Column<int>(type: "int", nullable: false),
                    TipoRequerimiento = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CondicionAcademica = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Correlatividades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Correlatividades_Materias_MateriaDestinoId",
                        column: x => x.MateriaDestinoId,
                        principalTable: "Materias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Correlatividades_Materias_MateriaRequisitoId",
                        column: x => x.MateriaRequisitoId,
                        principalTable: "Materias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Encuestas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MateriaId = table.Column<int>(type: "int", nullable: false),
                    DocenteId = table.Column<int>(type: "int", nullable: false),
                    Preguntas = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Activa = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Encuestas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Encuestas_Docentes_DocenteId",
                        column: x => x.DocenteId,
                        principalTable: "Docentes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Encuestas_Materias_MateriaId",
                        column: x => x.MateriaId,
                        principalTable: "Materias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EspaciosCurriculares",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MateriaId = table.Column<int>(type: "int", nullable: false),
                    DocenteId = table.Column<int>(type: "int", nullable: false),
                    CursoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EspaciosCurriculares", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EspaciosCurriculares_Cursos_CursoId",
                        column: x => x.CursoId,
                        principalTable: "Cursos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EspaciosCurriculares_Docentes_DocenteId",
                        column: x => x.DocenteId,
                        principalTable: "Docentes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EspaciosCurriculares_Materias_MateriaId",
                        column: x => x.MateriaId,
                        principalTable: "Materias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Examenes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MateriaId = table.Column<int>(type: "int", nullable: false),
                    FechaExamen = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Horario = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Cupo = table.Column<int>(type: "int", nullable: false),
                    TipoExamen = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Examenes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Examenes_Materias_MateriaId",
                        column: x => x.MateriaId,
                        principalTable: "Materias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HistorialAcademico",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EstudianteId = table.Column<int>(type: "int", nullable: false),
                    MateriaId = table.Column<int>(type: "int", nullable: false),
                    CursoId = table.Column<int>(type: "int", nullable: false),
                    Anio = table.Column<int>(type: "int", nullable: false),
                    Comision = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EstadoFinal = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NotaFinal = table.Column<decimal>(type: "decimal(4,2)", nullable: true),
                    Condicion = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialAcademico", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistorialAcademico_Cursos_CursoId",
                        column: x => x.CursoId,
                        principalTable: "Cursos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistorialAcademico_Estudiantes_EstudianteId",
                        column: x => x.EstudianteId,
                        principalTable: "Estudiantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistorialAcademico_Materias_MateriaId",
                        column: x => x.MateriaId,
                        principalTable: "Materias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InscripcionesMateria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EstudianteId = table.Column<int>(type: "int", nullable: false),
                    MateriaId = table.Column<int>(type: "int", nullable: false),
                    CursoId = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaInscripcion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InscripcionesMateria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InscripcionesMateria_Cursos_CursoId",
                        column: x => x.CursoId,
                        principalTable: "Cursos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InscripcionesMateria_Estudiantes_EstudianteId",
                        column: x => x.EstudianteId,
                        principalTable: "Estudiantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InscripcionesMateria_Materias_MateriaId",
                        column: x => x.MateriaId,
                        principalTable: "Materias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RespuestasEncuesta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EncuestaId = table.Column<int>(type: "int", nullable: false),
                    Preguntas = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Respuestas = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RespuestasEncuesta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RespuestasEncuesta_Encuestas_EncuestaId",
                        column: x => x.EncuestaId,
                        principalTable: "Encuestas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InscripcionesExamen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EstudianteId = table.Column<int>(type: "int", nullable: false),
                    ExamenId = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NotaValor = table.Column<decimal>(type: "decimal(4,2)", nullable: true),
                    FechaInscripcion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InscripcionesExamen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InscripcionesExamen_Estudiantes_EstudianteId",
                        column: x => x.EstudianteId,
                        principalTable: "Estudiantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InscripcionesExamen_Examenes_ExamenId",
                        column: x => x.ExamenId,
                        principalTable: "Examenes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Alertas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Anio = table.Column<int>(type: "int", nullable: false),
                    Comision = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EstadoFinal = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NotaFinal = table.Column<decimal>(type: "decimal(4,2)", nullable: true),
                    Condicion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Enviada = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InscripcionExamenId = table.Column<int>(type: "int", nullable: true),
                    InscripcionMateriaId = table.Column<int>(type: "int", nullable: true),
                    ExamenId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alertas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Alertas_Examenes_ExamenId",
                        column: x => x.ExamenId,
                        principalTable: "Examenes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Alertas_InscripcionesExamen_InscripcionExamenId",
                        column: x => x.InscripcionExamenId,
                        principalTable: "InscripcionesExamen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Alertas_InscripcionesMateria_InscripcionMateriaId",
                        column: x => x.InscripcionMateriaId,
                        principalTable: "InscripcionesMateria",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AuditoriaCambios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TablaAfectada = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RegistroAfectado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Accion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FechaCambio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValorAnterior = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValorNuevo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    ExamenId = table.Column<int>(type: "int", nullable: true),
                    CalendarioId = table.Column<int>(type: "int", nullable: true),
                    InscripcionExamenId = table.Column<int>(type: "int", nullable: true),
                    InscripcionMateriaId = table.Column<int>(type: "int", nullable: true),
                    EncuestaId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditoriaCambios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditoriaCambios_CalendarioAcademico_CalendarioId",
                        column: x => x.CalendarioId,
                        principalTable: "CalendarioAcademico",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AuditoriaCambios_Encuestas_EncuestaId",
                        column: x => x.EncuestaId,
                        principalTable: "Encuestas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AuditoriaCambios_Examenes_ExamenId",
                        column: x => x.ExamenId,
                        principalTable: "Examenes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AuditoriaCambios_InscripcionesExamen_InscripcionExamenId",
                        column: x => x.InscripcionExamenId,
                        principalTable: "InscripcionesExamen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AuditoriaCambios_InscripcionesMateria_InscripcionMateriaId",
                        column: x => x.InscripcionMateriaId,
                        principalTable: "InscripcionesMateria",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AuditoriaCambios_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alertas_ExamenId",
                table: "Alertas",
                column: "ExamenId");

            migrationBuilder.CreateIndex(
                name: "IX_Alertas_InscripcionExamenId",
                table: "Alertas",
                column: "InscripcionExamenId");

            migrationBuilder.CreateIndex(
                name: "IX_Alertas_InscripcionMateriaId",
                table: "Alertas",
                column: "InscripcionMateriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Asistencias_CursoId",
                table: "Asistencias",
                column: "CursoId");

            migrationBuilder.CreateIndex(
                name: "IX_Asistencias_EstudianteId_MateriaId_CursoId_Fecha",
                table: "Asistencias",
                columns: new[] { "EstudianteId", "MateriaId", "CursoId", "Fecha" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Asistencias_MateriaId",
                table: "Asistencias",
                column: "MateriaId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditoriaCambios_CalendarioId",
                table: "AuditoriaCambios",
                column: "CalendarioId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditoriaCambios_EncuestaId",
                table: "AuditoriaCambios",
                column: "EncuestaId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditoriaCambios_ExamenId",
                table: "AuditoriaCambios",
                column: "ExamenId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditoriaCambios_FechaCambio",
                table: "AuditoriaCambios",
                column: "FechaCambio");

            migrationBuilder.CreateIndex(
                name: "IX_AuditoriaCambios_InscripcionExamenId",
                table: "AuditoriaCambios",
                column: "InscripcionExamenId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditoriaCambios_InscripcionMateriaId",
                table: "AuditoriaCambios",
                column: "InscripcionMateriaId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditoriaCambios_TablaAfectada",
                table: "AuditoriaCambios",
                column: "TablaAfectada");

            migrationBuilder.CreateIndex(
                name: "IX_AuditoriaCambios_UsuarioId",
                table: "AuditoriaCambios",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarioAcademico_CursoId",
                table: "CalendarioAcademico",
                column: "CursoId");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarioAcademico_MateriaId",
                table: "CalendarioAcademico",
                column: "MateriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Correlatividades_MateriaDestinoId_MateriaRequisitoId_TipoRequerimiento",
                table: "Correlatividades",
                columns: new[] { "MateriaDestinoId", "MateriaRequisitoId", "TipoRequerimiento" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Correlatividades_MateriaRequisitoId",
                table: "Correlatividades",
                column: "MateriaRequisitoId");

            migrationBuilder.CreateIndex(
                name: "IX_Cursos_Anio_AnioLectivo_Comision",
                table: "Cursos",
                columns: new[] { "Anio", "AnioLectivo", "Comision" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cursos_PreceptorId",
                table: "Cursos",
                column: "PreceptorId");

            migrationBuilder.CreateIndex(
                name: "IX_Encuestas_DocenteId",
                table: "Encuestas",
                column: "DocenteId");

            migrationBuilder.CreateIndex(
                name: "IX_Encuestas_MateriaId",
                table: "Encuestas",
                column: "MateriaId");

            migrationBuilder.CreateIndex(
                name: "IX_EspaciosCurriculares_CursoId",
                table: "EspaciosCurriculares",
                column: "CursoId");

            migrationBuilder.CreateIndex(
                name: "IX_EspaciosCurriculares_DocenteId",
                table: "EspaciosCurriculares",
                column: "DocenteId");

            migrationBuilder.CreateIndex(
                name: "IX_EspaciosCurriculares_MateriaId_DocenteId_CursoId",
                table: "EspaciosCurriculares",
                columns: new[] { "MateriaId", "DocenteId", "CursoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Examenes_MateriaId",
                table: "Examenes",
                column: "MateriaId");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialAcademico_CursoId",
                table: "HistorialAcademico",
                column: "CursoId");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialAcademico_EstudianteId",
                table: "HistorialAcademico",
                column: "EstudianteId");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialAcademico_MateriaId",
                table: "HistorialAcademico",
                column: "MateriaId");

            migrationBuilder.CreateIndex(
                name: "IX_InscripcionesExamen_EstudianteId_ExamenId",
                table: "InscripcionesExamen",
                columns: new[] { "EstudianteId", "ExamenId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InscripcionesExamen_ExamenId",
                table: "InscripcionesExamen",
                column: "ExamenId");

            migrationBuilder.CreateIndex(
                name: "IX_InscripcionesMateria_CursoId",
                table: "InscripcionesMateria",
                column: "CursoId");

            migrationBuilder.CreateIndex(
                name: "IX_InscripcionesMateria_EstudianteId_MateriaId_CursoId",
                table: "InscripcionesMateria",
                columns: new[] { "EstudianteId", "MateriaId", "CursoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InscripcionesMateria_MateriaId",
                table: "InscripcionesMateria",
                column: "MateriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Materias_Codigo",
                table: "Materias",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RespuestasEncuesta_EncuestaId",
                table: "RespuestasEncuesta",
                column: "EncuestaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Alertas");

            migrationBuilder.DropTable(
                name: "Asistencias");

            migrationBuilder.DropTable(
                name: "AuditoriaCambios");

            migrationBuilder.DropTable(
                name: "Correlatividades");

            migrationBuilder.DropTable(
                name: "EspaciosCurriculares");

            migrationBuilder.DropTable(
                name: "HistorialAcademico");

            migrationBuilder.DropTable(
                name: "RespuestasEncuesta");

            migrationBuilder.DropTable(
                name: "CalendarioAcademico");

            migrationBuilder.DropTable(
                name: "InscripcionesExamen");

            migrationBuilder.DropTable(
                name: "InscripcionesMateria");

            migrationBuilder.DropTable(
                name: "Encuestas");

            migrationBuilder.DropTable(
                name: "Examenes");

            migrationBuilder.DropTable(
                name: "Cursos");

            migrationBuilder.DropTable(
                name: "Materias");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordResetToken",
                table: "Usuarios",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
