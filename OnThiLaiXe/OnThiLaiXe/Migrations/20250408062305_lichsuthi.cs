using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnThiLaiXe.Migrations
{
    /// <inheritdoc />
    public partial class lichsuthi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LichSuThis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BaiThiId = table.Column<int>(type: "int", nullable: false),
                    TenBaiThi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayThi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LoaiBaiThi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TongSoCau = table.Column<int>(type: "int", nullable: false),
                    SoCauDung = table.Column<int>(type: "int", nullable: false),
                    PhanTramDung = table.Column<double>(type: "float", nullable: false),
                    Diem = table.Column<int>(type: "int", nullable: false),
                    KetQua = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MacLoiNghiemTrong = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichSuThis", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LichSuThis");
        }
    }
}
