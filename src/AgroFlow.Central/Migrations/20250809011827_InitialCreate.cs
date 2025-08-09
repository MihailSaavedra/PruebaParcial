using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgroFlow.Central.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Agricultores",
                columns: table => new
                {
                    AgricultorId = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Finca = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Ubicacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Correo = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agricultores", x => x.AgricultorId);
                });

            migrationBuilder.CreateTable(
                name: "Cosechas",
                columns: table => new
                {
                    CosechaId = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    AgricultorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Producto = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Toneladas = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FacturaId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cosechas", x => x.CosechaId);
                    table.CheckConstraint("CK_Cosecha_Toneladas_Positive", "toneladas > 0");
                    table.ForeignKey(
                        name: "FK_Cosechas_Agricultores_AgricultorId",
                        column: x => x.AgricultorId,
                        principalTable: "Agricultores",
                        principalColumn: "AgricultorId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cosechas_AgricultorId",
                table: "Cosechas",
                column: "AgricultorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cosechas");

            migrationBuilder.DropTable(
                name: "Agricultores");
        }
    }
}
