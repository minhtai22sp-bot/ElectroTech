using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElectroTech.Infrastructure.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class AddBrandIdToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Brand",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ComId",
                table: "AuditLogs");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "AuditLogs",
                newName: "IpAddress");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "AuditLogs",
                newName: "EntityName");

            migrationBuilder.RenameColumn(
                name: "TableName",
                table: "AuditLogs",
                newName: "AdminName");

            migrationBuilder.RenameColumn(
                name: "PrimaryKey",
                table: "AuditLogs",
                newName: "AdminId");

            migrationBuilder.RenameColumn(
                name: "DateTime",
                table: "AuditLogs",
                newName: "CreatedOn");

            migrationBuilder.RenameColumn(
                name: "AffectedColumns",
                table: "AuditLogs",
                newName: "Action");

            migrationBuilder.AddColumn<int>(
                name: "BrandId",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EntityId",
                table: "AuditLogs",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Brands",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Slug = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Website = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Brands", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_BrandId",
                table: "Products",
                column: "BrandId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Brands_BrandId",
                table: "Products",
                column: "BrandId",
                principalTable: "Brands",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Brands_BrandId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "Brands");

            migrationBuilder.DropIndex(
                name: "IX_Products_BrandId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "BrandId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "EntityId",
                table: "AuditLogs");

            migrationBuilder.RenameColumn(
                name: "IpAddress",
                table: "AuditLogs",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "EntityName",
                table: "AuditLogs",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                table: "AuditLogs",
                newName: "DateTime");

            migrationBuilder.RenameColumn(
                name: "AdminName",
                table: "AuditLogs",
                newName: "TableName");

            migrationBuilder.RenameColumn(
                name: "AdminId",
                table: "AuditLogs",
                newName: "PrimaryKey");

            migrationBuilder.RenameColumn(
                name: "Action",
                table: "AuditLogs",
                newName: "AffectedColumns");

            migrationBuilder.AddColumn<string>(
                name: "Brand",
                table: "Products",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ComId",
                table: "AuditLogs",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
