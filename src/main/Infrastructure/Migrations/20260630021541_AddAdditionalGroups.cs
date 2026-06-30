using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAdditionalGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AdditionalsPrice",
                table: "OrderItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "AdditionalGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MinSelections = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MaxSelections = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdditionalGroups", x => x.Id);
                    table.CheckConstraint("CK_AdditionalGroups_MaxSelections_Positive", "[MaxSelections] >= 1");
                    table.CheckConstraint("CK_AdditionalGroups_MinMax", "[MinSelections] <= [MaxSelections]");
                    table.CheckConstraint("CK_AdditionalGroups_MinSelections_NonNegative", "[MinSelections] >= 0");
                    table.ForeignKey(
                        name: "FK_AdditionalGroups_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderItemAdditionals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdditionalItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GroupName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItemAdditionals", x => x.Id);
                    table.CheckConstraint("CK_OrderItemAdditionals_Quantity_Positive", "[Quantity] >= 1");
                    table.ForeignKey(
                        name: "FK_OrderItemAdditionals_OrderItems_OrderItemId",
                        column: x => x.OrderItemId,
                        principalTable: "OrderItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AdditionalItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdditionalItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdditionalItems_AdditionalGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "AdditionalGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalGroups_ProductId",
                table: "AdditionalGroups",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalItems_GroupId",
                table: "AdditionalItems",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemAdditionals_OrderItemId",
                table: "OrderItemAdditionals",
                column: "OrderItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdditionalItems");

            migrationBuilder.DropTable(
                name: "OrderItemAdditionals");

            migrationBuilder.DropTable(
                name: "AdditionalGroups");

            migrationBuilder.DropColumn(
                name: "AdditionalsPrice",
                table: "OrderItems");
        }
    }
}
