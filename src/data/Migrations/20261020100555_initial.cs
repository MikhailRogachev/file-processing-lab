using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace data.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AllowedFileTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Extension = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Chain = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(800)", maxLength: 800, nullable: false),
                    AssetType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AllowedFileTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MediaPackages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Identifier = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaPackages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Repeated = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(800)", maxLength: 800, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MediaAssets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Filename = table.Column<string>(type: "nvarchar(550)", maxLength: 550, nullable: false),
                    AssetType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CurrentStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    Etag = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    Result = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(800)", maxLength: 800, nullable: false),
                    MediaPackageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaAssets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MediaAssets_MediaPackages_MediaPackageId",
                        column: x => x.MediaPackageId,
                        principalTable: "MediaPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AllowedFileTypes_Extension",
                table: "AllowedFileTypes",
                column: "Extension",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MediaAssets_MediaPackageId_Filename",
                table: "MediaAssets",
                columns: new[] { "MediaPackageId", "Filename" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MediaPackages_Identifier",
                table: "MediaPackages",
                column: "Identifier",
                unique: true);

            migrationBuilder.Sql("INSERT INTO \"AllowedFileTypes\" (\"Id\", \"Extension\", \"MimeType\", \"Chain\", \"Comment\", \"AssetType\") VALUES " +
                "(1, '.jpg', 'image/jpeg', '', 'JPEG image file', 'Image'), " +
                "(2, '.png', 'image/png', '', 'PNG image file', 'Image'), " +
                "(3, '.gif', 'image/gif', '', 'GIF image file', 'Image'), " +
                "(4, '.mp4', 'video/mp4', '', 'MP4 video file', 'Video'), " +
                "(5, '.mov', 'video/quicktime', '', 'MOV video file', 'Video'), " +
                "(6, '.avi', 'video/x-msvideo', '', 'AVI video file', 'Video'), " +
                "(7, '.mkv', 'video/x-matroska', '', 'MKV video file', 'Video'), " +
                "(8, '.mp3', 'audio/mpeg', '', 'MP3 audio file', 'Audio'), " +
                "(9, '.wav', 'audio/wav', '', 'WAV audio file', 'Audio'), " +
                "(10, '.flac', 'audio/flac', '', 'FLAC audio file', 'Audio'), " +
                "(11, '.txt', 'text/plain', '', 'Txt text file', 'Subtitle'), " +
                "(12, '.slt', 'text/plain', '', 'Slt subtitle file', 'Subtitle'), " +
                "(13, '.srt', 'text/plain', '', 'Srt subtitle file', 'Subtitle'), " +
                "(14, '.vtt', 'text/vtt', '', 'VTT subtitle file', 'Subtitle');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AllowedFileTypes");

            migrationBuilder.DropTable(
                name: "MediaAssets");

            migrationBuilder.DropTable(
                name: "OutboxMessages");

            migrationBuilder.DropTable(
                name: "MediaPackages");
        }
    }
}
