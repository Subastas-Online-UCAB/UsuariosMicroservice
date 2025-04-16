using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UsuarioServicio.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class AddRolAndPrivilegio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Privilegios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NombreTabla = table.Column<string>(type: "text", nullable: false),
                    Operacion = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Privilegios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RolPrivilegios",
                columns: table => new
                {
                    RolId = table.Column<Guid>(type: "uuid", nullable: false),
                    PrivilegioId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolPrivilegios", x => new { x.RolId, x.PrivilegioId });
                    table.ForeignKey(
                        name: "FK_RolPrivilegios_Privilegios_PrivilegioId",
                        column: x => x.PrivilegioId,
                        principalTable: "Privilegios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolPrivilegios_Roles_RolId",
                        column: x => x.RolId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RolPrivilegios_PrivilegioId",
                table: "RolPrivilegios",
                column: "PrivilegioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RolPrivilegios");

            migrationBuilder.DropTable(
                name: "Privilegios");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
