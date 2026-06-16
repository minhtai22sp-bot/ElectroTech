using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElectroTech.Infrastructure.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class AddOrderIdToReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrderId",
                table: "Reviews",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TopProductDto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThumbnailUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SoldCount = table.Column<int>(type: "int", nullable: false),
                    Revenue = table.Column<decimal>(type: "decimal(18,3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TopProductDto", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TopProductDto");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "Reviews");
        }
    }
}
