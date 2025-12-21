using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ticket_Booking.Migrations
{
    /// <inheritdoc />
    public partial class AddRoundTripBookingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PriceLastUpdated",
                table: "Trips",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RoundTripDiscountPercent",
                table: "Trips",
                type: "TEXT",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "BookingGroupId",
                table: "Tickets",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OutboundTicketId",
                table: "Tickets",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReturnTicketId",
                table: "Tickets",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Tickets",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_BookingGroupId",
                table: "Tickets",
                column: "BookingGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_BookingGroupId_Type",
                table: "Tickets",
                columns: new[] { "BookingGroupId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_OutboundTicketId",
                table: "Tickets",
                column: "OutboundTicketId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ReturnTicketId",
                table: "Tickets",
                column: "ReturnTicketId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_Type",
                table: "Tickets",
                column: "Type");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Tickets_OutboundTicketId",
                table: "Tickets",
                column: "OutboundTicketId",
                principalTable: "Tickets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Tickets_ReturnTicketId",
                table: "Tickets",
                column: "ReturnTicketId",
                principalTable: "Tickets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Tickets_OutboundTicketId",
                table: "Tickets");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Tickets_ReturnTicketId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_BookingGroupId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_BookingGroupId_Type",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_OutboundTicketId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_ReturnTicketId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_Type",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "PriceLastUpdated",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "RoundTripDiscountPercent",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "BookingGroupId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "OutboundTicketId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "ReturnTicketId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Tickets");
        }
    }
}
