using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoutingService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Adding_Automatisation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrackedRoutes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FromAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ToAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastCheckedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackedRoutes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RouteSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrackedRouteId = table.Column<int>(type: "int", nullable: false),
                    CapturedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DurationInTrafficMinutes = table.Column<double>(type: "float", nullable: false),
                    DurationWithoutTrafficMinutes = table.Column<double>(type: "float", nullable: false),
                    TrafficLevel = table.Column<double>(type: "float", nullable: false),
                    AverageTaxiPrice = table.Column<double>(type: "float", nullable: false),
                    DistanceKm = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouteSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RouteSnapshots_TrackedRoutes_TrackedRouteId",
                        column: x => x.TrackedRouteId,
                        principalTable: "TrackedRoutes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RouteSnapshots_TrackedRouteId_CapturedAt",
                table: "RouteSnapshots",
                columns: new[] { "TrackedRouteId", "CapturedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RouteSnapshots");

            migrationBuilder.DropTable(
                name: "TrackedRoutes");
        }
    }
}
