using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace localshopyNew.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrderTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Discount",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ShopId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ShopName",
                table: "OrderItems");

            migrationBuilder.AddColumn<Guid>(
                name: "ShopId",
                table: "Orders",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "ShopName",
                table: "Orders",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShopId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShopName",
                table: "Orders");

            migrationBuilder.AddColumn<int>(
                name: "Discount",
                table: "Orders",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "OrderItems",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "ShopId",
                table: "OrderItems",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "ShopName",
                table: "OrderItems",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }
    }
}
