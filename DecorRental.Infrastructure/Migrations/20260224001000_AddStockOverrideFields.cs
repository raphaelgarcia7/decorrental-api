using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DecorRental.Infrastructure.Migrations;

[DbContext(typeof(Persistence.DecorRentalDbContext))]
[Migration("20260224001000_AddStockOverrideFields")]
public class AddStockOverrideFields : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "IsStockOverride",
            table: "Reservations",
            type: "INTEGER",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<string>(
            name: "StockOverrideReason",
            table: "Reservations",
            type: "TEXT",
            maxLength: 500,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "IsStockOverride",
            table: "Reservations");

        migrationBuilder.DropColumn(
            name: "StockOverrideReason",
            table: "Reservations");
    }
}
