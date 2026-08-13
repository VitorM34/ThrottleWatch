using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThrottleWatch.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMetricRollups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "metric_rollups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BucketStart = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Granularity = table.Column<byte>(type: "smallint", nullable: false),
                    TotalRequests = table.Column<long>(type: "bigint", nullable: false),
                    BlockedRequests = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_metric_rollups", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_metric_rollups_BucketStart",
                table: "metric_rollups",
                column: "BucketStart");

            migrationBuilder.CreateIndex(
                name: "IX_metric_rollups_Granularity_BucketStart",
                table: "metric_rollups",
                columns: new[] { "Granularity", "BucketStart" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "metric_rollups");
        }
    }
}
