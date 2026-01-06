using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ticket_Booking.Migrations
{
    /// <inheritdoc />
    public partial class SyncModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketChangeHistories_Tickets_TicketId",
                table: "TicketChangeHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_TicketChangeHistories_Users_ChangedByUserId",
                table: "TicketChangeHistories");

            migrationBuilder.DropIndex(
                name: "IX_TicketChangeHistories_ChangedByUserId",
                table: "TicketChangeHistories");

            migrationBuilder.DropColumn(
                name: "AmountPaid",
                table: "TicketChangeHistories");

            migrationBuilder.DropColumn(
                name: "AmountRefunded",
                table: "TicketChangeHistories");

            migrationBuilder.DropColumn(
                name: "ChangedAt",
                table: "TicketChangeHistories");

            migrationBuilder.DropColumn(
                name: "ChangedByUserId",
                table: "TicketChangeHistories");

            migrationBuilder.DropColumn(
                name: "NewPrice",
                table: "TicketChangeHistories");

            migrationBuilder.DropColumn(
                name: "NewSeatClass",
                table: "TicketChangeHistories");

            migrationBuilder.DropColumn(
                name: "NewTripId",
                table: "TicketChangeHistories");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "TicketChangeHistories");

            migrationBuilder.DropColumn(
                name: "OriginalPrice",
                table: "TicketChangeHistories");

            migrationBuilder.RenameColumn(
                name: "TicketId",
                table: "TicketChangeHistories",
                newName: "OriginalTicketId");

            migrationBuilder.RenameColumn(
                name: "OriginalTripId",
                table: "TicketChangeHistories",
                newName: "NewTicketId");

            migrationBuilder.RenameColumn(
                name: "OriginalSeatClass",
                table: "TicketChangeHistories",
                newName: "ChangeDate");

            migrationBuilder.RenameIndex(
                name: "IX_TicketChangeHistories_TicketId",
                table: "TicketChangeHistories",
                newName: "IX_TicketChangeHistories_OriginalTicketId");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "TicketChangeHistories",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmountPaid",
                table: "TicketChangeHistories",
                type: "TEXT",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_TicketChangeHistories_NewTicketId",
                table: "TicketChangeHistories",
                column: "NewTicketId");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketChangeHistories_Tickets_NewTicketId",
                table: "TicketChangeHistories",
                column: "NewTicketId",
                principalTable: "Tickets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TicketChangeHistories_Tickets_OriginalTicketId",
                table: "TicketChangeHistories",
                column: "OriginalTicketId",
                principalTable: "Tickets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketChangeHistories_Tickets_NewTicketId",
                table: "TicketChangeHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_TicketChangeHistories_Tickets_OriginalTicketId",
                table: "TicketChangeHistories");

            migrationBuilder.DropIndex(
                name: "IX_TicketChangeHistories_NewTicketId",
                table: "TicketChangeHistories");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "TicketChangeHistories");

            migrationBuilder.DropColumn(
                name: "TotalAmountPaid",
                table: "TicketChangeHistories");

            migrationBuilder.RenameColumn(
                name: "OriginalTicketId",
                table: "TicketChangeHistories",
                newName: "TicketId");

            migrationBuilder.RenameColumn(
                name: "NewTicketId",
                table: "TicketChangeHistories",
                newName: "OriginalTripId");

            migrationBuilder.RenameColumn(
                name: "ChangeDate",
                table: "TicketChangeHistories",
                newName: "OriginalSeatClass");

            migrationBuilder.RenameIndex(
                name: "IX_TicketChangeHistories_OriginalTicketId",
                table: "TicketChangeHistories",
                newName: "IX_TicketChangeHistories_TicketId");

            migrationBuilder.AddColumn<decimal>(
                name: "AmountPaid",
                table: "TicketChangeHistories",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AmountRefunded",
                table: "TicketChangeHistories",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "ChangedAt",
                table: "TicketChangeHistories",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "ChangedByUserId",
                table: "TicketChangeHistories",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "NewPrice",
                table: "TicketChangeHistories",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "NewSeatClass",
                table: "TicketChangeHistories",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "NewTripId",
                table: "TicketChangeHistories",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "TicketChangeHistories",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OriginalPrice",
                table: "TicketChangeHistories",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_TicketChangeHistories_ChangedByUserId",
                table: "TicketChangeHistories",
                column: "ChangedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketChangeHistories_Tickets_TicketId",
                table: "TicketChangeHistories",
                column: "TicketId",
                principalTable: "Tickets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TicketChangeHistories_Users_ChangedByUserId",
                table: "TicketChangeHistories",
                column: "ChangedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
