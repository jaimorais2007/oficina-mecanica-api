using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OficinaApi.Migrations
{
    /// <inheritdoc />
    public partial class AddInactiveFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Inactive",
                table: "Vehicles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Inactive",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Inactive",
                table: "Services",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Inactive",
                table: "ServiceOrderStatuses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Inactive",
                table: "ServiceOrderServices",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Inactive",
                table: "ServiceOrders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Inactive",
                table: "ServiceOrderParts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Inactive",
                table: "Parts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Inactive",
                table: "Customers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Inactive",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Inactive",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Inactive",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "Inactive",
                table: "ServiceOrderStatuses");

            migrationBuilder.DropColumn(
                name: "Inactive",
                table: "ServiceOrderServices");

            migrationBuilder.DropColumn(
                name: "Inactive",
                table: "ServiceOrders");

            migrationBuilder.DropColumn(
                name: "Inactive",
                table: "ServiceOrderParts");

            migrationBuilder.DropColumn(
                name: "Inactive",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "Inactive",
                table: "Customers");
        }
    }
}
