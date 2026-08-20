using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThrottleWatch.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantIsolation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_metric_rollups_Granularity_BucketStart",
                table: "metric_rollups");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "metric_rollups",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "default");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "metric_entries",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "default");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "insights",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "default");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "alert_rules",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "default");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "alert_events",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "default");

            migrationBuilder.CreateIndex(
                name: "IX_metric_rollups_TenantId",
                table: "metric_rollups",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_metric_rollups_TenantId_Granularity_BucketStart",
                table: "metric_rollups",
                columns: new[] { "TenantId", "Granularity", "BucketStart" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_metric_entries_TenantId",
                table: "metric_entries",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_metric_entries_TenantId_Timestamp",
                table: "metric_entries",
                columns: new[] { "TenantId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_insights_TenantId",
                table: "insights",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_alert_rules_TenantId",
                table: "alert_rules",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_alert_events_TenantId",
                table: "alert_events",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_metric_rollups_TenantId",
                table: "metric_rollups");

            migrationBuilder.DropIndex(
                name: "IX_metric_rollups_TenantId_Granularity_BucketStart",
                table: "metric_rollups");

            migrationBuilder.DropIndex(
                name: "IX_metric_entries_TenantId",
                table: "metric_entries");

            migrationBuilder.DropIndex(
                name: "IX_metric_entries_TenantId_Timestamp",
                table: "metric_entries");

            migrationBuilder.DropIndex(
                name: "IX_insights_TenantId",
                table: "insights");

            migrationBuilder.DropIndex(
                name: "IX_alert_rules_TenantId",
                table: "alert_rules");

            migrationBuilder.DropIndex(
                name: "IX_alert_events_TenantId",
                table: "alert_events");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "metric_rollups");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "metric_entries");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "insights");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "alert_rules");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "alert_events");

            migrationBuilder.CreateIndex(
                name: "IX_metric_rollups_Granularity_BucketStart",
                table: "metric_rollups",
                columns: new[] { "Granularity", "BucketStart" },
                unique: true);
        }
    }
}
