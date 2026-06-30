using Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260630124500_RemoveDeliveryAverageSpeedSetting")]
    public partial class RemoveDeliveryAverageSpeedSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryAverageSpeedKmH",
                table: "Estabelecimentos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DeliveryAverageSpeedKmH",
                table: "Estabelecimentos",
                type: "int",
                nullable: false,
                defaultValue: 25);
        }
    }
}
