using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SwipeMate.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRecipeFilters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RecipeSessionFilters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Complexity = table.Column<int>(type: "integer", nullable: true),
                    Cuisine = table.Column<string>(type: "text", nullable: true),
                    FoodType = table.Column<string>(type: "text", nullable: true),
                    BudgetLevel = table.Column<int>(type: "integer", nullable: true),
                    MinRating = table.Column<double>(type: "double precision", nullable: true),
                    IngredientsCsv = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeSessionFilters", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecipeSessionFilters");
        }
    }
}
