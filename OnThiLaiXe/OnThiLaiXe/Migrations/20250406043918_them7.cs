using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnThiLaiXe.Migrations
{
    /// <inheritdoc />
    public partial class them7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BaiThis_AspNetUsers_UserId",
                table: "BaiThis");

            migrationBuilder.DropTable(
                name: "KetQuaBaiThis");

            migrationBuilder.DropIndex(
                name: "IX_BaiThis_UserId",
                table: "BaiThis");

            migrationBuilder.DropColumn(
                name: "NguoiDungId",
                table: "BaiThis");

            migrationBuilder.AddColumn<int>(
                name: "DiemToiThieu",
                table: "LoaiBangLais",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ThoiGianThi",
                table: "LoaiBangLais",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "LoaiCauHoi",
                table: "CauHois",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "BaiThis",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "DaHoanThanh",
                table: "BaiThis",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "KetQua",
                table: "BaiThis",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LoaiBaiThi",
                table: "BaiThis",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "PhanTramDung",
                table: "BaiThis",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "SoCauChuaTraLoi",
                table: "BaiThis",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SoCauDung",
                table: "BaiThis",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SoCauSai",
                table: "BaiThis",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TenBaiThi",
                table: "BaiThis",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiemToiThieu",
                table: "LoaiBangLais");

            migrationBuilder.DropColumn(
                name: "ThoiGianThi",
                table: "LoaiBangLais");

            migrationBuilder.DropColumn(
                name: "LoaiCauHoi",
                table: "CauHois");

            migrationBuilder.DropColumn(
                name: "DaHoanThanh",
                table: "BaiThis");

            migrationBuilder.DropColumn(
                name: "KetQua",
                table: "BaiThis");

            migrationBuilder.DropColumn(
                name: "LoaiBaiThi",
                table: "BaiThis");

            migrationBuilder.DropColumn(
                name: "PhanTramDung",
                table: "BaiThis");

            migrationBuilder.DropColumn(
                name: "SoCauChuaTraLoi",
                table: "BaiThis");

            migrationBuilder.DropColumn(
                name: "SoCauDung",
                table: "BaiThis");

            migrationBuilder.DropColumn(
                name: "SoCauSai",
                table: "BaiThis");

            migrationBuilder.DropColumn(
                name: "TenBaiThi",
                table: "BaiThis");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "BaiThis",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NguoiDungId",
                table: "BaiThis",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "KetQuaBaiThis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BaiThiId = table.Column<int>(type: "int", nullable: false),
                    CauHoiId = table.Column<int>(type: "int", nullable: false),
                    CauTraLoi = table.Column<string>(type: "nvarchar(1)", nullable: false),
                    DungSai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KetQuaBaiThis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KetQuaBaiThis_BaiThis_BaiThiId",
                        column: x => x.BaiThiId,
                        principalTable: "BaiThis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KetQuaBaiThis_CauHois_CauHoiId",
                        column: x => x.CauHoiId,
                        principalTable: "CauHois",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BaiThis_UserId",
                table: "BaiThis",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_KetQuaBaiThis_BaiThiId",
                table: "KetQuaBaiThis",
                column: "BaiThiId");

            migrationBuilder.CreateIndex(
                name: "IX_KetQuaBaiThis_CauHoiId",
                table: "KetQuaBaiThis",
                column: "CauHoiId");

            migrationBuilder.AddForeignKey(
                name: "FK_BaiThis_AspNetUsers_UserId",
                table: "BaiThis",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
