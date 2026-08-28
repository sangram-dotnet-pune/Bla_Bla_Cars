using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileAndVehicle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MiniBio",
                table: "AppUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TravelPreferences",
                table: "AppUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VehicleColor",
                table: "AppUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VehicleLicensePlate",
                table: "AppUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VehicleMake",
                table: "AppUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VehicleModel",
                table: "AppUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VehicleYear",
                table: "AppUsers",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MiniBio",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "TravelPreferences",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "VehicleColor",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "VehicleLicensePlate",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "VehicleMake",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "VehicleModel",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "VehicleYear",
                table: "AppUsers");
        }
    }
}
