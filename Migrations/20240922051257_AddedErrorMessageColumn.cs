using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImageProcessor.Migrations
{
    /// <inheritdoc />
    public partial class AddedErrorMessageColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StatusError",
                table: "OrderItems",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StatusError",
                table: "OrderItems");
        }
    }
}
