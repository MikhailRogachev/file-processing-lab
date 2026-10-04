using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace data.Migrations
{
    /// <inheritdoc />
    public partial class addallowedFileTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AllowedFileTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Extension = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    MimeType = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Comment = table.Column<string>(type: "character varying(800)", maxLength: 800, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AllowedFileTypes", x => x.Id);
                });

            migrationBuilder.Sql("INSERT INTO \"AllowedFileTypes\" (\"Id\", \"Extension\", \"MimeType\", \"IsActive\", \"Comment\") VALUES " +
                "(1, '.jpg', 'image/jpeg', true, 'JPEG image file'), " +
                "(2, '.png', 'image/png', true, 'PNG image file'), " +
                "(3, '.gif', 'image/gif', true, 'GIF image file'), " +
                "(4, '.mp4', 'video/mp4', true, 'MP4 video file'), " +
                "(5, '.mov', 'video/quicktime', true, 'MOV video file'), " +
                "(6, '.avi', 'video/x-msvideo', true, 'AVI video file'), " +
                "(7, '.mkv', 'video/x-matroska', true, 'MKV video file'), " +
                "(8, '.mp3', 'audio/mpeg', true, 'MP3 audio file'), " +
                "(9, '.wav', 'audio/wav', true, 'WAV audio file'), " +
                "(10, '.flac', 'audio/flac', true, 'FLAC audio file');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AllowedFileTypes");
        }
    }
}
