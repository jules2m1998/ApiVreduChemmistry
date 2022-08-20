using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiVrEdu.Migrations
{
    /// <inheritdoc />
    public partial class elementcreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Color",
                table: "Elements");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Elements",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
