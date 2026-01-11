using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazorApp2.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketCostAndDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeviceType",
                table: "Tickets",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Diagnostic",
                table: "Tickets",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiagnosticFee",
                table: "Tickets",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LaborCost",
                table: "Tickets",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PartsCost",
                table: "Tickets",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalCost",
                table: "Tickets",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeviceType",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "Diagnostic",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "DiagnosticFee",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "LaborCost",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "PartsCost",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "TotalCost",
                table: "Tickets");
        }
    }
}
