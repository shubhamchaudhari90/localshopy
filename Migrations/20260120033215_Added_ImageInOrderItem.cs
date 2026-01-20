using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace localshopyNew.Migrations
{
    /// <inheritdoc />
    public partial class Added_ImageInOrderItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageFileName",
                table: "OrderItems",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageFileName",
                table: "OrderItems");
        }
    }
}
