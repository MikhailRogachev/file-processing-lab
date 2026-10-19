using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace data.Migrations
{
    /// <inheritdoc />
    public partial class updateallowedtypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AssetType",
                table: "AllowedFileTypes",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);


            migrationBuilder.Sql("INSERT INTO \"AllowedFileTypes\" (\"Id\", \"Extension\", \"MimeType\", \"Chain\", \"Comment\", \"AssetType\") VALUES " +
                "(11, '.txt', 'text/plain', '', 'Txt text file', 'Subtitle'), " +
                "(12, '.slt', 'text/plain', '', 'Slt subtitle file', 'Subtitle'), " +
                "(13, '.srt', 'text/plain', '', 'Srt subtitle file', 'Subtitle'), " +
                "(14, '.vtt', 'text/vtt', '', 'VTT subtitle file', 'Subtitle');");

            // 3. Batched AssetType Updates (Fixed 'Aduio' typo for Id = 8)
            migrationBuilder.Sql("UPDATE [AllowedFileTypes] SET [AssetType] = 'Image' WHERE [Id] IN (1, 2, 3);");
            migrationBuilder.Sql("UPDATE [AllowedFileTypes] SET [AssetType] = 'Video' WHERE [Id] IN (4, 5, 6, 7);");
            migrationBuilder.Sql("UPDATE [AllowedFileTypes] SET [AssetType] = 'Audio' WHERE [Id] IN (8, 9, 10);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AssetType",
                table: "AllowedFileTypes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);
        }
    }
}
