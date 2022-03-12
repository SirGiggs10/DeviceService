using Microsoft.EntityFrameworkCore.Migrations;

namespace DeviceService.Core.Data.Migrations
{
    public partial class AuditReportPKUpdated : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AuditReport",
                table: "AuditReport");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AuditReport",
                table: "AuditReport",
                column: "AuditReportId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditReport_AuditReportActivityId",
                table: "AuditReport",
                column: "AuditReportActivityId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AuditReport",
                table: "AuditReport");

            migrationBuilder.DropIndex(
                name: "IX_AuditReport_AuditReportActivityId",
                table: "AuditReport");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AuditReport",
                table: "AuditReport",
                column: "AuditReportActivityId");
        }
    }
}
