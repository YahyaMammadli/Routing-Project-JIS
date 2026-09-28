using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoutingService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddingMultiProvider : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AverageTaxiPrice",
                table: "RouteLogs");

            migrationBuilder.DropColumn(
                name: "DurationInTrafficMinutes",
                table: "RouteLogs");

            migrationBuilder.RenameColumn(
                name: "TrafficLevel",
                table: "RouteLogs",
                newName: "DurationMinutes");

            migrationBuilder.AlterColumn<double>(
                name: "DurationWithoutTrafficMinutes",
                table: "RouteSnapshots",
                type: "float",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AddColumn<double>(
                name: "DurationMinutes",
                table: "RouteSnapshots",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "Provider",
                table: "RouteSnapshots",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "TrafficAvailable",
                table: "RouteSnapshots",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TrafficUsed",
                table: "RouteSnapshots",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TransportMode",
                table: "RouteSnapshots",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<double>(
                name: "DurationWithoutTrafficMinutes",
                table: "RouteLogs",
                type: "float",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AddColumn<string>(
                name: "Error",
                table: "RouteLogs",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Provider",
                table: "RouteLogs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "TrafficAvailable",
                table: "RouteLogs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TrafficUsed",
                table: "RouteLogs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TransportMode",
                table: "RouteLogs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_RouteLogs_RequestedAt_Provider_TransportMode",
                table: "RouteLogs",
                columns: new[] { "RequestedAt", "Provider", "TransportMode" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RouteLogs_RequestedAt_Provider_TransportMode",
                table: "RouteLogs");

            migrationBuilder.DropColumn(
                name: "DurationMinutes",
                table: "RouteSnapshots");

            migrationBuilder.DropColumn(
                name: "Provider",
                table: "RouteSnapshots");

            migrationBuilder.DropColumn(
                name: "TrafficAvailable",
                table: "RouteSnapshots");

            migrationBuilder.DropColumn(
                name: "TrafficUsed",
                table: "RouteSnapshots");

            migrationBuilder.DropColumn(
                name: "TransportMode",
                table: "RouteSnapshots");

            migrationBuilder.DropColumn(
                name: "Error",
                table: "RouteLogs");

            migrationBuilder.DropColumn(
                name: "Provider",
                table: "RouteLogs");

            migrationBuilder.DropColumn(
                name: "TrafficAvailable",
                table: "RouteLogs");

            migrationBuilder.DropColumn(
                name: "TrafficUsed",
                table: "RouteLogs");

            migrationBuilder.DropColumn(
                name: "TransportMode",
                table: "RouteLogs");

            migrationBuilder.RenameColumn(
                name: "DurationMinutes",
                table: "RouteLogs",
                newName: "TrafficLevel");

            migrationBuilder.AlterColumn<double>(
                name: "DurationWithoutTrafficMinutes",
                table: "RouteSnapshots",
                type: "float",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "DurationWithoutTrafficMinutes",
                table: "RouteLogs",
                type: "float",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AddColumn<double>(
                name: "AverageTaxiPrice",
                table: "RouteLogs",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "DurationInTrafficMinutes",
                table: "RouteLogs",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
