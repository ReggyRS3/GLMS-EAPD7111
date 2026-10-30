using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GLMS.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLogAndManagerAlerts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContractAuditLogs",
                columns: table => new
                {
                    ContractAuditLogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContractId = table.Column<int>(type: "int", nullable: false),
                    OldStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NewStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractAuditLogs", x => x.ContractAuditLogId);
                    table.ForeignKey(
                        name: "FK_ContractAuditLogs_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "ContractId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ManagerAlerts",
                columns: table => new
                {
                    ManagerAlertId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContractId = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDismissed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManagerAlerts", x => x.ManagerAlertId);
                    table.ForeignKey(
                        name: "FK_ManagerAlerts_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "ContractId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContractAuditLogs_ContractId",
                table: "ContractAuditLogs",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_ManagerAlerts_ContractId",
                table: "ManagerAlerts",
                column: "ContractId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContractAuditLogs");

            migrationBuilder.DropTable(
                name: "ManagerAlerts");
        }
    }
}
