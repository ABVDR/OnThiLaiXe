using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnThiLaiXe.Migrations
{
    /// <inheritdoc />
    public partial class them8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CauTrucDeThis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoaiBangLaiId = table.Column<int>(type: "int", nullable: false),
                    ChuDeId = table.Column<int>(type: "int", nullable: false),
                    SoLuongCauHoi = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CauTrucDeThis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CauTrucDeThis_ChuDes_ChuDeId",
                        column: x => x.ChuDeId,
                        principalTable: "ChuDes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CauTrucDeThis_LoaiBangLais_LoaiBangLaiId",
                        column: x => x.LoaiBangLaiId,
                        principalTable: "LoaiBangLais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CauTrucDeThis_ChuDeId",
                table: "CauTrucDeThis",
                column: "ChuDeId");

            migrationBuilder.CreateIndex(
                name: "IX_CauTrucDeThis_LoaiBangLaiId",
                table: "CauTrucDeThis",
                column: "LoaiBangLaiId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CauTrucDeThis");
        }
    }
}
