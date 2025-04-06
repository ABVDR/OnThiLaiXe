using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnThiLaiXe.Migrations
{
    /// <inheritdoc />
    public partial class Them : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BaiThis_AspNetUsers_UserId",
                table: "BaiThis");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "BaiThis",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_BaiThis_AspNetUsers_UserId",
                table: "BaiThis",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BaiThis_AspNetUsers_UserId",
                table: "BaiThis");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "BaiThis",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BaiThis_AspNetUsers_UserId",
                table: "BaiThis",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
