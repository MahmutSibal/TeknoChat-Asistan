using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeknofestAsistan.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEscalationReason : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EscalationReason",
                table: "ChatQueries",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EscalationReason",
                table: "ChatQueries");
        }
    }
}
