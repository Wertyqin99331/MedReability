using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedReability.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExerciseMediaUrlsArray : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string[]>(
                name: "media_urls",
                table: "exercises",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.Sql("""
                UPDATE exercises
                SET media_urls = ARRAY[media_url]::text[]
                WHERE media_url IS NOT NULL AND media_url <> '';
                """);

            migrationBuilder.DropColumn(
                name: "media_url",
                table: "exercises");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "media_url",
                table: "exercises",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE exercises
                SET media_url = media_urls[1]
                WHERE COALESCE(array_length(media_urls, 1), 0) > 0;
                """);

            migrationBuilder.DropColumn(
                name: "media_urls",
                table: "exercises");
        }
    }
}
