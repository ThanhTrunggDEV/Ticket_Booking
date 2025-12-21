using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ticket_Booking.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckInFieldsToTicket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tickets_TripId",
                table: "Tickets");

            migrationBuilder.AddColumn<string>(
                name: "BoardingPassUrl",
                table: "Tickets",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckInTime",
                table: "Tickets",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCheckedIn",
                table: "Tickets",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_IsCheckedIn",
                table: "Tickets",
                column: "IsCheckedIn");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_TripId_SeatNumber",
                table: "Tickets",
                columns: new[] { "TripId", "SeatNumber" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tickets_IsCheckedIn",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_TripId_SeatNumber",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "BoardingPassUrl",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "CheckInTime",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "IsCheckedIn",
                table: "Tickets");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_TripId",
                table: "Tickets",
                column: "TripId");
        }
    }
}
