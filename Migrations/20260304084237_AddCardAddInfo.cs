using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BotApiTemplate.Migrations
{
    /// <inheritdoc />
    public partial class AddCardAddInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AddInfo",
                table: "Cards",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddInfo",
                table: "Cards");
        }
    }
}
