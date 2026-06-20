using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueOrderNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Number",
                table: "Orders",
                type: "nvarchar(19)",
                maxLength: 19,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.Sql(
                """
                ;WITH NumberedOrders AS (
                    SELECT
                        [Id],
                        [Number],
                        ROW_NUMBER() OVER (PARTITION BY [Number] ORDER BY [CreatedAt], [Id]) AS [RowNumber]
                    FROM [Orders]
                )
                UPDATE [Orders]
                SET [Number] = CONCAT('LEG', RIGHT(REPLACE(CONVERT(varchar(36), [Orders].[Id]), '-', ''), 16))
                FROM [Orders]
                INNER JOIN [NumberedOrders] ON [Orders].[Id] = [NumberedOrders].[Id]
                WHERE [NumberedOrders].[RowNumber] > 1;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Number",
                table: "Orders",
                column: "Number",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_Number",
                table: "Orders");

            migrationBuilder.AlterColumn<string>(
                name: "Number",
                table: "Orders",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(19)",
                oldMaxLength: 19);
        }
    }
}
