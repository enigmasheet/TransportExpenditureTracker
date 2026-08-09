using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransportExpenditureTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ShortName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    RegistrationNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    PanNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    VatNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Province = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    District = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Municipality = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Ward = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Website = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    LogoPath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DefaultVatRate = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    FiscalYearStartMonth = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Currency = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    CurrencySymbol = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Companies");
        }
    }
}
