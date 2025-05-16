using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnThiLaiXe.Migrations
{
    /// <inheritdoc />
    public partial class suacauhoisai : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "CauHoiSais",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CauHoiSais_AspNetUsers_UserId",
                table: "CauHoiSais");

            migrationBuilder.DropIndex(
                name: "IX_CauHoiSais_UserId",
                table: "CauHoiSais");

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
