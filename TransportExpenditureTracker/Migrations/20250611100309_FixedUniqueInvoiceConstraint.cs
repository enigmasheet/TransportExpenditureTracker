using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransportExpenditureTracker.Migrations
{
    /// <inheritdoc />
    public partial class FixedUniqueInvoiceConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Invoices_InvoiceNo_PartyId_CompanyId",
                table: "Invoices",
                columns: new[] { "InvoiceNo", "PartyId", "CompanyId" },
                unique: true,
                filter: "[CompanyId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Invoices_InvoiceNo_PartyId_CompanyId",
                table: "Invoices");
        }
    }
}
