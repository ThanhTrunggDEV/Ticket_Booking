using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ticket_Booking.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelAfterAddOns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AddOnPrice",
                table: "Tickets",
                type: "TEXT",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "BaggageOption",
                table: "Tickets",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MealOption",
                table: "Tickets",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddOnPrice",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "BaggageOption",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "MealOption",
                table: "Tickets");
        }
    }
}
