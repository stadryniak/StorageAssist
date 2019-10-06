using Microsoft.EntityFrameworkCore.Migrations;

namespace StorageAssist.Migrations
{
    public partial class manymanyfix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserCommonResource_CommonResources_CommonResourceId",
                table: "UserCommonResource");

            migrationBuilder.DropForeignKey(
                name: "FK_UserCommonResource_AspNetUsers_UserId",
                table: "UserCommonResource");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_UserCommonResource_UserCommonResourceId",
                table: "UserCommonResource");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserCommonResource",
                table: "UserCommonResource");

            migrationBuilder.AlterColumn<string>(
                name: "CommonResourceId",
                table: "UserCommonResource",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserCommonResource",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserCommonResource",
                table: "UserCommonResource",
                column: "UserCommonResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCommonResource_UserId",
                table: "UserCommonResource",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserCommonResource_CommonResources_CommonResourceId",
                table: "UserCommonResource",
                column: "CommonResourceId",
                principalTable: "CommonResources",
                principalColumn: "CommonResourceId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserCommonResource_AspNetUsers_UserId",
                table: "UserCommonResource",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserCommonResource_CommonResources_CommonResourceId",
                table: "UserCommonResource");

            migrationBuilder.DropForeignKey(
                name: "FK_UserCommonResource_AspNetUsers_UserId",
                table: "UserCommonResource");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserCommonResource",
                table: "UserCommonResource");

            migrationBuilder.DropIndex(
                name: "IX_UserCommonResource_UserId",
                table: "UserCommonResource");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserCommonResource",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CommonResourceId",
                table: "UserCommonResource",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_UserCommonResource_UserCommonResourceId",
                table: "UserCommonResource",
                column: "UserCommonResourceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserCommonResource",
                table: "UserCommonResource",
                columns: new[] { "UserId", "CommonResourceId" });

            migrationBuilder.AddForeignKey(
                name: "FK_UserCommonResource_CommonResources_CommonResourceId",
                table: "UserCommonResource",
                column: "CommonResourceId",
                principalTable: "CommonResources",
                principalColumn: "CommonResourceId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserCommonResource_AspNetUsers_UserId",
                table: "UserCommonResource",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
