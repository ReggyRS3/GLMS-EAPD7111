using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GLMS.Web.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    ClientId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ContactEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Region = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.ClientId);
                });

            migrationBuilder.CreateTable(
                name: "Contracts",
                columns: table => new
                {
                    ContractId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ServiceLevel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SignedAgreementPath = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contracts", x => x.ContractId);
                    table.ForeignKey(
                        name: "FK_Contracts_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceRequests",
                columns: table => new
                {
                    ServiceRequestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContractId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CostUsd = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ExchangeRate = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    CostZar = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceRequests", x => x.ServiceRequestId);
                    table.ForeignKey(
                        name: "FK_ServiceRequests_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "ContractId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Clients",
                columns: new[] { "ClientId", "ContactEmail", "ContactPhone", "Name", "Region" },
                values: new object[,]
                {
                    { 1, "ops@kalaharifreight.co.za", "0115550101", "Kalahari Freight", "Africa" },
                    { 2, "info@rhinecargo.de", "+4930555012", "Rhine Cargo GmbH", "Europe" }
                });

            migrationBuilder.InsertData(
                table: "Contracts",
                columns: new[] { "ContractId", "ClientId", "EndDate", "ServiceLevel", "SignedAgreementPath", "StartDate", "Status" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2026, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "Premium", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active" },
                    { 2, 1, new DateTime(2025, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "Standard", null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Expired" },
                    { 3, 2, new DateTime(2027, 2, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "Express", null, new DateTime(2026, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OnHold" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_ClientId",
                table: "Contracts",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_ContractId",
                table: "ServiceRequests",
                column: "ContractId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceRequests");

            migrationBuilder.DropTable(
                name: "Contracts");

            migrationBuilder.DropTable(
                name: "Clients");
        }
    }
}
