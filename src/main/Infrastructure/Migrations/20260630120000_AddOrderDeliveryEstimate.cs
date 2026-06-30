using System;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260630120000_AddOrderDeliveryEstimate")]
    public partial class AddOrderDeliveryEstimate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PreparationTimeMinutes",
                table: "Estabelecimentos",
                type: "int",
                nullable: false,
                defaultValue: 30);

            migrationBuilder.AddColumn<int>(
                name: "DeliveryAverageSpeedKmH",
                table: "Estabelecimentos",
                type: "int",
                nullable: false,
                defaultValue: 25);

            migrationBuilder.AddColumn<int>(
                name: "DeliverySafetyMarginMinutes",
                table: "Estabelecimentos",
                type: "int",
                nullable: false,
                defaultValue: 10);

            migrationBuilder.AddColumn<int>(
                name: "EstimatedPreparationMinutes",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EstimatedTravelMinutes",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EstimatedDeliveryMinutes",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedDeliveryDistanceKm",
                table: "Orders",
                type: "decimal(8,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EstimatedReadyAt",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EstimatedDeliveryDeadlineAt",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MarkedDelayedAt",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_EstimatedDeliveryDeadlineAt",
                table: "Orders",
                column: "EstimatedDeliveryDeadlineAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_EstimatedDeliveryDeadlineAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PreparationTimeMinutes",
                table: "Estabelecimentos");

            migrationBuilder.DropColumn(
                name: "DeliveryAverageSpeedKmH",
                table: "Estabelecimentos");

            migrationBuilder.DropColumn(
                name: "DeliverySafetyMarginMinutes",
                table: "Estabelecimentos");

            migrationBuilder.DropColumn(
                name: "EstimatedPreparationMinutes",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "EstimatedTravelMinutes",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "EstimatedDeliveryMinutes",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "EstimatedDeliveryDistanceKm",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "EstimatedReadyAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "EstimatedDeliveryDeadlineAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "MarkedDelayedAt",
                table: "Orders");
        }
    }
}
