using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnThiLaiXe.Migrations
{
    /// <inheritdoc />
    public partial class sua2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChiTietLichSuThis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LichSuThiId = table.Column<int>(type: "int", nullable: false),
                    CauHoiId = table.Column<int>(type: "int", nullable: false),
                    CauTraLoi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DungSai = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietLichSuThis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChiTietLichSuThis_CauHois_CauHoiId",
                        column: x => x.CauHoiId,
                        principalTable: "CauHois",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChiTietLichSuThis_LichSuThis_LichSuThiId",
                        column: x => x.LichSuThiId,
                        principalTable: "LichSuThis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietLichSuThis_CauHoiId",
                table: "ChiTietLichSuThis",
                column: "CauHoiId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietLichSuThis_LichSuThiId",
                table: "ChiTietLichSuThis",
                column: "LichSuThiId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChiTietLichSuThis");
        }
    }
}
