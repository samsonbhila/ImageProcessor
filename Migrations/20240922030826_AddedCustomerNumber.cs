using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImageProcessor.Migrations
{
    /// <inheritdoc />
    public partial class AddedCustomerNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "CustomerNo",
                table: "Orders",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerNo",
                table: "Orders");
        }
    }
}
