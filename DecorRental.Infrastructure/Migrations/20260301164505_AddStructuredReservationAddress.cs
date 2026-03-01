using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DecorRental.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStructuredReservationAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomerCity",
                table: "Reservations",
                type: "TEXT",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerComplement",
                table: "Reservations",
                type: "TEXT",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerNeighborhood",
                table: "Reservations",
                type: "TEXT",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerNumber",
                table: "Reservations",
                type: "TEXT",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerReference",
                table: "Reservations",
                type: "TEXT",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerState",
                table: "Reservations",
                type: "TEXT",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerStreet",
                table: "Reservations",
                type: "TEXT",
                maxLength: 180,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerZipCode",
                table: "Reservations",
                type: "TEXT",
                maxLength: 8,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerCity",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "CustomerComplement",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "CustomerNeighborhood",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "CustomerNumber",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "CustomerReference",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "CustomerState",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "CustomerStreet",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "CustomerZipCode",
                table: "Reservations");
        }
    }
}
