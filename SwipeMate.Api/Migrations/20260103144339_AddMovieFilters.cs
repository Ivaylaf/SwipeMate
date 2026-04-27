using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SwipeMate.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddMovieFilters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MovieSessionFilters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    MinRating = table.Column<double>(type: "double precision", nullable: true),
                    YearFrom = table.Column<int>(type: "integer", nullable: true),
                    YearTo = table.Column<int>(type: "integer", nullable: true),
                    GenresCsv = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovieSessionFilters", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MovieSessionFilters");
        }
    }
}
