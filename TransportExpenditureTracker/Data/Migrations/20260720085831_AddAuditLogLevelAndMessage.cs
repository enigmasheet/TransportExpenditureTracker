using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransportExpenditureTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLogLevelAndMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExpenseHeaders_Suppliers_SupplierId",
                table: "ExpenseHeaders");

            migrationBuilder.DropColumn(
                name: "FiscalYear",
                table: "ExpenseHeaders");

            migrationBuilder.AddColumn<string>(
                name: "LogLevel",
                table: "AuditLogs",
                type: "TEXT",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "AuditLogs",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RemoteIp",
                table: "AuditLogs",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExportQueues_Status",
                table: "ExportQueues",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseHeaders_CreatedAt",
                table: "ExpenseHeaders",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseHeaders_EnglishDate",
                table: "ExpenseHeaders",
                column: "EnglishDate");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityName_EntityId",
                table: "AuditLogs",
                columns: new[] { "EntityName", "EntityId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ExpenseHeaders_Suppliers_SupplierId",
                table: "ExpenseHeaders",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "SupplierId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExpenseHeaders_Suppliers_SupplierId",
                table: "ExpenseHeaders");

            migrationBuilder.DropIndex(
                name: "IX_ExportQueues_Status",
                table: "ExportQueues");

            migrationBuilder.DropIndex(
                name: "IX_ExpenseHeaders_CreatedAt",
                table: "ExpenseHeaders");

            migrationBuilder.DropIndex(
                name: "IX_ExpenseHeaders_EnglishDate",
                table: "ExpenseHeaders");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_EntityName_EntityId",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "LogLevel",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "Message",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "RemoteIp",
                table: "AuditLogs");

            migrationBuilder.AddColumn<string>(
                name: "FiscalYear",
                table: "ExpenseHeaders",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_ExpenseHeaders_Suppliers_SupplierId",
                table: "ExpenseHeaders",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "SupplierId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
