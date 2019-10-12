using Microsoft.EntityFrameworkCore.Migrations;

namespace StorageAssist.Migrations
{
    public partial class floatToDouble : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "QuantityWeight",
                table: "Products",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<float>(
                name: "QuantityWeight",
                table: "Products",
                type: "real",
                nullable: true,
                oldClrType: typeof(double),
                oldNullable: true);
        }
    }
}
