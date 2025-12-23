using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ticket_Booking.Migrations
{
    /// <inheritdoc />
    public partial class AddPassengerNameToTicket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add column as nullable first
            migrationBuilder.AddColumn<string>(
                name: "PassengerName",
                table: "Tickets",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            // Populate existing tickets with User's FullName
            migrationBuilder.Sql(@"
                UPDATE Tickets
                SET PassengerName = (
                    SELECT FullName 
                    FROM Users 
                    WHERE Users.Id = Tickets.UserId
                )
                WHERE PassengerName IS NULL;
            ");

            // Make column non-nullable after populating
            migrationBuilder.AlterColumn<string>(
                name: "PassengerName",
                table: "Tickets",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PassengerName",
                table: "Tickets");
        }
    }
}
