using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransportExpenditureTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPerUserOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ExpenseHeaders_InvoiceNo_SupplierId_FiscalYearId",
                table: "ExpenseHeaders");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "ExportQueues",
                type: "TEXT",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "ExpenseHeaders",
                type: "TEXT",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ExportQueues_UserId",
                table: "ExportQueues",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseHeaders_InvoiceNo_SupplierId_FiscalYearId_UserId",
                table: "ExpenseHeaders",
                columns: new[] { "InvoiceNo", "SupplierId", "FiscalYearId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseHeaders_UserId",
                table: "ExpenseHeaders",
                column: "UserId");

            migrationBuilder.Sql(
                """
                UPDATE ExpenseHeaders
                SET UserId = (
                    SELECT u.Id
                    FROM AspNetUsers u
                    INNER JOIN AspNetUserRoles ur ON ur.UserId = u.Id
                    INNER JOIN AspNetRoles r ON r.Id = ur.RoleId
                    WHERE r.Name IN ('SuperAdmin', 'Admin')
                    ORDER BY u.Id
                    LIMIT 1
                )
                WHERE UserId = '';

                UPDATE ExportQueues
                SET UserId = (
                    SELECT u.Id
                    FROM AspNetUsers u
                    INNER JOIN AspNetUserRoles ur ON ur.UserId = u.Id
                    INNER JOIN AspNetRoles r ON r.Id = ur.RoleId
                    WHERE r.Name IN ('SuperAdmin', 'Admin')
                    ORDER BY u.Id
                    LIMIT 1
                )
                WHERE UserId = '';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ExportQueues_UserId",
                table: "ExportQueues");

            migrationBuilder.DropIndex(
                name: "IX_ExpenseHeaders_InvoiceNo_SupplierId_FiscalYearId_UserId",
                table: "ExpenseHeaders");

            migrationBuilder.DropIndex(
                name: "IX_ExpenseHeaders_UserId",
                table: "ExpenseHeaders");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ExportQueues");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ExpenseHeaders");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseHeaders_InvoiceNo_SupplierId_FiscalYearId",
                table: "ExpenseHeaders",
                columns: new[] { "InvoiceNo", "SupplierId", "FiscalYearId" },
                unique: true);
        }
    }
}
