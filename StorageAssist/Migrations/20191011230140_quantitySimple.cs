using Microsoft.EntityFrameworkCore.Migrations;

namespace StorageAssist.Migrations
{
    public partial class quantitySimple : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuantityCount",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "QuantityWeight",
                table: "Products");

            migrationBuilder.AddColumn<double>(
                name: "Quantity",
                table: "Products",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "Products");

            migrationBuilder.AddColumn<int>(
                name: "QuantityCount",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "QuantityWeight",
                table: "Products",
                type: "float",
                nullable: true);
        }
    }
}
