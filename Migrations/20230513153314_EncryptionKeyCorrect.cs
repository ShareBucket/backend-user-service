using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserMicroService.Migrations
{
    /// <inheritdoc />
    public partial class EncryptionKeyCorrect : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EncryptionKey",
                table: "Metadatas");

            migrationBuilder.AddColumn<byte[]>(
                name: "EncryptionKey",
                table: "MemoryAreas",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EncryptionKey",
                table: "MemoryAreas");

            migrationBuilder.AddColumn<byte[]>(
                name: "EncryptionKey",
                table: "Metadatas",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}
