using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PracticaProfesional.Migrations
{
    /// <inheritdoc />
    public partial class AddLogSeguridad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LogsSeguridad",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email       = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Exitoso     = table.Column<bool>(type: "bit", nullable: false),
                    MotivoFallo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IpOrigen    = table.Column<string>(type: "nvarchar(45)",  maxLength: 45,  nullable: false),
                    UserAgent   = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Timestamp   = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogsSeguridad", x => x.Id);
                });

            migrationBuilder.CreateIndex("IX_LogsSeguridad_Email",     "LogsSeguridad", "Email");
            migrationBuilder.CreateIndex("IX_LogsSeguridad_Exitoso",   "LogsSeguridad", "Exitoso");
            migrationBuilder.CreateIndex("IX_LogsSeguridad_Timestamp", "LogsSeguridad", "Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "LogsSeguridad");
        }
    }
}
