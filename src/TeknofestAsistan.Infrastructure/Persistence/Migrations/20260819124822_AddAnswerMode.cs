using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeknofestAsistan.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAnswerMode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AnswerMode",
                table: "ChatQueries",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnswerMode",
                table: "ChatQueries");
        }
    }
}
