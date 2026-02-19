using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SwipeMate.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddBoardGameFilters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BoardGameSessionFilters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    GameType = table.Column<string>(type: "text", nullable: true),
                    DurationMin = table.Column<int>(type: "integer", nullable: true),
                    DurationMax = table.Column<int>(type: "integer", nullable: true),
                    PlayersMin = table.Column<int>(type: "integer", nullable: true),
                    PlayersMax = table.Column<int>(type: "integer", nullable: true),
                    MinRating = table.Column<double>(type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoardGameSessionFilters", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BoardGameSessionFilters");
        }
    }
}
