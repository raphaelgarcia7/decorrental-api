using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DecorRental.Infrastructure.Persistence;

public sealed class DecorRentalDbContextFactory : IDesignTimeDbContextFactory<DecorRentalDbContext>
{
    public DecorRentalDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DecorRentalDbContext>();
        optionsBuilder.UseSqlite("Data Source=decorental-design-time.db");

        return new DecorRentalDbContext(optionsBuilder.Options);
    }
}
