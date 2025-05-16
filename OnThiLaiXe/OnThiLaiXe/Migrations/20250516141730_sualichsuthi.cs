using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnThiLaiXe.Migrations
{
    /// <inheritdoc />
    public partial class sualichsuthi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "LichSuThis",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "CauHoiSais",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuThis_UserId",
                table: "LichSuThis",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CauHoiSais_UserId",
                table: "CauHoiSais",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CauHoiSais_AspNetUsers_UserId",
                table: "CauHoiSais",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LichSuThis_AspNetUsers_UserId",
                table: "LichSuThis",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CauHoiSais_AspNetUsers_UserId",
                table: "CauHoiSais");

            migrationBuilder.DropForeignKey(
                name: "FK_LichSuThis_AspNetUsers_UserId",
                table: "LichSuThis");

            migrationBuilder.DropIndex(
                name: "IX_LichSuThis_UserId",
                table: "LichSuThis");

            migrationBuilder.DropIndex(
                name: "IX_CauHoiSais_UserId",
                table: "CauHoiSais");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "LichSuThis",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "CauHoiSais",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
