using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransportExpenditureTracker.Migrations
{
    /// <inheritdoc />
    public partial class AddedNepaliDateToInvoiceTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NepaliMiti",
                table: "Invoices",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NepaliMiti",
                table: "Invoices");
        }
    }
}
