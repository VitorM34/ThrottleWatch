using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThrottleWatch.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "alert_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AlertRuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TriggeredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsAcknowledged = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alert_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "alert_rules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Condition = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Threshold = table.Column<double>(type: "double precision", nullable: false),
                    Severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CooldownMinutes = table.Column<int>(type: "integer", nullable: false),
                    LastTriggeredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alert_rules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "insights",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AffectedResource = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    GeneratedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDismissed = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_insights", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "metric_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Path = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Method = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    StatusCode = table.Column<int>(type: "integer", nullable: false),
                    IsBlocked = table.Column<bool>(type: "boolean", nullable: false),
                    PolicyName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ClientIp = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ApiKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    DurationMs = table.Column<double>(type: "double precision", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_metric_entries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_alert_events_AlertRuleId",
                table: "alert_events",
                column: "AlertRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_alert_events_TriggeredAt",
                table: "alert_events",
                column: "TriggeredAt");

            migrationBuilder.CreateIndex(
                name: "IX_alert_rules_IsEnabled",
                table: "alert_rules",
                column: "IsEnabled");

            migrationBuilder.CreateIndex(
                name: "IX_insights_GeneratedAt",
                table: "insights",
                column: "GeneratedAt");

            migrationBuilder.CreateIndex(
                name: "IX_insights_IsDismissed",
                table: "insights",
                column: "IsDismissed");

            migrationBuilder.CreateIndex(
                name: "IX_metric_entries_ClientIp",
                table: "metric_entries",
                column: "ClientIp");

            migrationBuilder.CreateIndex(
                name: "IX_metric_entries_IsBlocked",
                table: "metric_entries",
                column: "IsBlocked");

            migrationBuilder.CreateIndex(
                name: "IX_metric_entries_Path_Method",
                table: "metric_entries",
                columns: new[] { "Path", "Method" });

            migrationBuilder.CreateIndex(
                name: "IX_metric_entries_Timestamp",
                table: "metric_entries",
                column: "Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "alert_events");

            migrationBuilder.DropTable(
                name: "alert_rules");

            migrationBuilder.DropTable(
                name: "insights");

            migrationBuilder.DropTable(
                name: "metric_entries");
        }
    }
}
