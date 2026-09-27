using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Demo1.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSecondaryProductImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SecondaryImagePath",
                table: "Products",
                type: "TEXT",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SecondaryImagePath",
                table: "Products");
        }
    }
}
