using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SensorRuleEngine.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Alerts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RuleId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DeviceId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Metric = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    StartTimestamp = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    EndTimestamp = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    PeakValue = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alerts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Readings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DeviceId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Metric = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Value = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    Sequence = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Readings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RuleResults",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RuleId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DeviceId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Metric = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Sequence = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RuleResults", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_RuleId_DeviceId_Metric_StartTimestamp_EndTimestamp",
                table: "Alerts",
                columns: new[] { "RuleId", "DeviceId", "Metric", "StartTimestamp", "EndTimestamp" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Readings_DeviceId_Metric_Timestamp_Sequence",
                table: "Readings",
                columns: new[] { "DeviceId", "Metric", "Timestamp", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RuleResults_RuleId_DeviceId_Metric_Timestamp_Sequence",
                table: "RuleResults",
                columns: new[] { "RuleId", "DeviceId", "Metric", "Timestamp", "Sequence" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Alerts");

            migrationBuilder.DropTable(
                name: "Readings");

            migrationBuilder.DropTable(
                name: "RuleResults");
        }
    }
}
