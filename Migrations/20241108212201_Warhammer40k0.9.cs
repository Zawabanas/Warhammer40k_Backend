using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarHammer40K.Migrations
{
    /// <inheritdoc />
    public partial class Warhammer40k09 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "Characters",
                newName: "Imagen");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Imagen",
                table: "Characters",
                newName: "ImageUrl");
        }
    }
}
