using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SensorRuleEngine.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReadingAcceptableStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAcceptable",
                table: "Readings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAcceptable",
                table: "Readings");
        }
    }
}
