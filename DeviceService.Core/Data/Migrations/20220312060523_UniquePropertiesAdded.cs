using Microsoft.EntityFrameworkCore.Migrations;

namespace DeviceService.Core.Data.Migrations
{
    public partial class UniquePropertiesAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ProjectModuleName",
                table: "ProjectModule",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RoleName",
                table: "FunctionalityRole",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "FunctionalityName",
                table: "FunctionalityRole",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "FunctionalityName",
                table: "Functionality",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DeviceTypeName",
                table: "DeviceType",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DeviceOperationName",
                table: "DeviceOperation",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DeviceName",
                table: "Device",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectModule_ProjectModuleName",
                table: "ProjectModule",
                column: "ProjectModuleName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FunctionalityRole_FunctionalityName_RoleName",
                table: "FunctionalityRole",
                columns: new[] { "FunctionalityName", "RoleName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Functionality_FunctionalityName",
                table: "Functionality",
                column: "FunctionalityName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceType_DeviceTypeName",
                table: "DeviceType",
                column: "DeviceTypeName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceOperation_DeviceOperationName",
                table: "DeviceOperation",
                column: "DeviceOperationName",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProjectModule_ProjectModuleName",
                table: "ProjectModule");

            migrationBuilder.DropIndex(
                name: "IX_FunctionalityRole_FunctionalityName_RoleName",
                table: "FunctionalityRole");

            migrationBuilder.DropIndex(
                name: "IX_Functionality_FunctionalityName",
                table: "Functionality");

            migrationBuilder.DropIndex(
                name: "IX_DeviceType_DeviceTypeName",
                table: "DeviceType");

            migrationBuilder.DropIndex(
                name: "IX_DeviceOperation_DeviceOperationName",
                table: "DeviceOperation");

            migrationBuilder.AlterColumn<string>(
                name: "ProjectModuleName",
                table: "ProjectModule",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "RoleName",
                table: "FunctionalityRole",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "FunctionalityName",
                table: "FunctionalityRole",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "FunctionalityName",
                table: "Functionality",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "DeviceTypeName",
                table: "DeviceType",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "DeviceOperationName",
                table: "DeviceOperation",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "DeviceName",
                table: "Device",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
