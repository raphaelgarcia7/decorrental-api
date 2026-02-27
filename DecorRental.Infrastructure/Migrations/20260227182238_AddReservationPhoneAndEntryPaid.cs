using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DecorRental.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReservationPhoneAndEntryPaid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomerPhoneNumber",
                table: "Reservations",
                type: "TEXT",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsEntryPaid",
                table: "Reservations",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerPhoneNumber",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "IsEntryPaid",
                table: "Reservations");
        }
    }
}
