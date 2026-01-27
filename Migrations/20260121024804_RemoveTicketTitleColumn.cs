using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazorApp2.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTicketTitleColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Title",
                table: "Tickets");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "FirstTimeCustomers",
                newName: "Created");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Customers",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Customers");

            migrationBuilder.RenameColumn(
                name: "Created",
                table: "FirstTimeCustomers",
                newName: "CreatedAt");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Tickets",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
