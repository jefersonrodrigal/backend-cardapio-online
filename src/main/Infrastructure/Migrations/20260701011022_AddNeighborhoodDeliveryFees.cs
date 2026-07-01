using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNeighborhoodDeliveryFees : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NeighborhoodDeliveryFees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Neighborhood = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Fee = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NeighborhoodDeliveryFees", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NeighborhoodDeliveryFees_Neighborhood",
                table: "NeighborhoodDeliveryFees",
                column: "Neighborhood",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NeighborhoodDeliveryFees");
        }
    }
}
