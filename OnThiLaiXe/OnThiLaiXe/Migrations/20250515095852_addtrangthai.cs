using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnThiLaiXe.Migrations
{
    /// <inheritdoc />
    public partial class addtrangthai : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isDeleted",
                table: "CauHois",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isDeleted",
                table: "CauHois");
        }
    }
}
