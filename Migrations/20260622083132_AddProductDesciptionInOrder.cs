using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace localshopyNew.Migrations
{
    /// <inheritdoc />
    public partial class AddProductDesciptionInOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShopDescription",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "SpecialInstructions",
                table: "Products");

            migrationBuilder.AddColumn<string>(
                name: "ProductDescription",
                table: "OrderItems",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductDescription",
                table: "OrderItems");

            migrationBuilder.AddColumn<string>(
                name: "ShopDescription",
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
    }
}
