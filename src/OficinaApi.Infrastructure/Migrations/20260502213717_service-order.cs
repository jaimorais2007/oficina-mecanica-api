using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OficinaApi.Migrations
{
    /// <inheritdoc />
    public partial class serviceorder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientCpf",
                table: "ServiceOrders");

            migrationBuilder.DropColumn(
                name: "FinishedExecutionAt",
                table: "ServiceOrders");

            migrationBuilder.DropColumn(
                name: "StartedExecutionAt",
                table: "ServiceOrders");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ServiceOrders");

            migrationBuilder.DropColumn(
                name: "VehiclePlate",
                table: "ServiceOrders");

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "Vehicles",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<decimal>(
                name: "DefaultPrice",
                table: "Services",
                type: "numeric(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)");

            migrationBuilder.AddColumn<decimal>(
                name: "Budget",
                table: "ServiceOrders",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "ServiceOrders",
                type: "uuid",
                maxLength: 14,
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "VehicleId",
                table: "ServiceOrders",
                type: "uuid",
                maxLength: 14,
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "PersonType",
                table: "Customers",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateTable(
                name: "ServiceOrderParts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    StockQuantityWasEnsured = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceOrderParts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceOrderParts_Parts_PartId",
                        column: x => x.PartId,
                        principalTable: "Parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceOrderParts_ServiceOrders_ServiceOrderId",
                        column: x => x.ServiceOrderId,
                        principalTable: "ServiceOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceOrderServices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceOrderServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceOrderServices_ServiceOrders_ServiceOrderId",
                        column: x => x.ServiceOrderId,
                        principalTable: "ServiceOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceOrderServices_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceOrderStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ServiceOrderId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceOrderStatuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceOrderStatuses_ServiceOrders_ServiceOrderId",
                        column: x => x.ServiceOrderId,
                        principalTable: "ServiceOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_CustomerId",
                table: "Vehicles",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOrders_CustomerId",
                table: "ServiceOrders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOrders_VehicleId",
                table: "ServiceOrders",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOrderParts_PartId",
                table: "ServiceOrderParts",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOrderParts_ServiceOrderId",
                table: "ServiceOrderParts",
                column: "ServiceOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOrderServices_ServiceId",
                table: "ServiceOrderServices",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOrderServices_ServiceOrderId",
                table: "ServiceOrderServices",
                column: "ServiceOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOrderStatuses_ServiceOrderId",
                table: "ServiceOrderStatuses",
                column: "ServiceOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceOrders_Customers_CustomerId",
                table: "ServiceOrders",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceOrders_Vehicles_VehicleId",
                table: "ServiceOrders",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_Customers_CustomerId",
                table: "Vehicles",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceOrders_Customers_CustomerId",
                table: "ServiceOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceOrders_Vehicles_VehicleId",
                table: "ServiceOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_Customers_CustomerId",
                table: "Vehicles");

            migrationBuilder.DropTable(
                name: "ServiceOrderParts");

            migrationBuilder.DropTable(
                name: "ServiceOrderServices");

            migrationBuilder.DropTable(
                name: "ServiceOrderStatuses");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_CustomerId",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_ServiceOrders_CustomerId",
                table: "ServiceOrders");

            migrationBuilder.DropIndex(
                name: "IX_ServiceOrders_VehicleId",
                table: "ServiceOrders");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Budget",
                table: "ServiceOrders");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "ServiceOrders");

            migrationBuilder.DropColumn(
                name: "VehicleId",
                table: "ServiceOrders");

            migrationBuilder.AlterColumn<decimal>(
                name: "DefaultPrice",
                table: "Services",
                type: "numeric(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)");

            migrationBuilder.AddColumn<string>(
                name: "ClientCpf",
                table: "ServiceOrders",
                type: "character varying(14)",
                maxLength: 14,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "FinishedExecutionAt",
                table: "ServiceOrders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartedExecutionAt",
                table: "ServiceOrders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "ServiceOrders",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VehiclePlate",
                table: "ServiceOrders",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "PersonType",
                table: "Customers",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
