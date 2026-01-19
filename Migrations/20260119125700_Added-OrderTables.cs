using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace localshopyNew.Migrations
{
    /// <inheritdoc />
    public partial class AddedOrderTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EmailId = table.Column<string>(type: "TEXT", nullable: false),
                    OrderNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Subtotal = table.Column<int>(type: "INTEGER", nullable: false),
                    Tax = table.Column<int>(type: "INTEGER", nullable: false),
                    ShippingFee = table.Column<int>(type: "INTEGER", nullable: false),
                    Discount = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalAmount = table.Column<int>(type: "INTEGER", nullable: false),
                    PaymentMethod = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    PaymentStatus = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ShippingAddress = table.Column<string>(type: "TEXT", nullable: false),
                    BillingAddress = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrderId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ShopId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ShopName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ProductName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Location = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    UnitPrice = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalPrice = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "Orders");
        }
    }
}
