using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace localshopyNew.Migrations
{
    /// <inheritdoc />
    public partial class AddShopTagLineAndInstrctionFOrProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShopDescription",
                table: "Shops",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TagLine",
                table: "Shops",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SpecialInstructions",
                table: "Products",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShopDescription",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "TagLine",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "SpecialInstructions",
                table: "Products");
        }
    }
}
