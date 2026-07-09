using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransportExpenditureTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFiscalYearIdToExpenseHeader : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ExpenseHeaders_InvoiceNo_SupplierId",
                table: "ExpenseHeaders");

            migrationBuilder.AddColumn<int>(
                name: "FiscalYearId",
                table: "ExpenseHeaders",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseHeaders_FiscalYearId",
                table: "ExpenseHeaders",
                column: "FiscalYearId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseHeaders_InvoiceNo_SupplierId_FiscalYearId",
                table: "ExpenseHeaders",
                columns: new[] { "InvoiceNo", "SupplierId", "FiscalYearId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ExpenseHeaders_FiscalYears_FiscalYearId",
                table: "ExpenseHeaders",
                column: "FiscalYearId",
                principalTable: "FiscalYears",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExpenseHeaders_FiscalYears_FiscalYearId",
                table: "ExpenseHeaders");

            migrationBuilder.DropIndex(
                name: "IX_ExpenseHeaders_FiscalYearId",
                table: "ExpenseHeaders");

            migrationBuilder.DropIndex(
                name: "IX_ExpenseHeaders_InvoiceNo_SupplierId_FiscalYearId",
                table: "ExpenseHeaders");

            migrationBuilder.DropColumn(
                name: "FiscalYearId",
                table: "ExpenseHeaders");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseHeaders_InvoiceNo_SupplierId",
                table: "ExpenseHeaders",
                columns: new[] { "InvoiceNo", "SupplierId" },
                unique: true);
        }
    }
}
