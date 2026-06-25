using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddCategories : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Categories",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Slug = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                SortOrder = table.Column<int>(type: "int", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Categories", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Categories_Slug",
            table: "Categories",
            column: "Slug",
            unique: true);

        migrationBuilder.InsertData(
            table: "Categories",
            columns: new[] { "Slug", "Name", "SortOrder", "IsActive" },
            values: new object[,]
            {
                { "hamburguer", "Hamburguer", 1, true },
                { "pizza", "Pizza", 2, true },
                { "bebida", "Bebida", 3, true },
                { "sobremesa", "Sobremesa", 4, true },
                { "porcao", "Porcao", 5, true },
                { "outro", "Outro", 6, true }
            });

        // Normaliza slugs existentes em Products para minusculas
        migrationBuilder.Sql("UPDATE [Products] SET [Category] = LOWER([Category])");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Categories");
    }
}
