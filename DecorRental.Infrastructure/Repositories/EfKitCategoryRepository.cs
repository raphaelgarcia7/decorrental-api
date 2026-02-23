using DecorRental.Domain.Entities;
using DecorRental.Domain.Repositories;
using DecorRental.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DecorRental.Infrastructure.Repositories;

public sealed class EfKitCategoryRepository : IKitCategoryRepository
{
    private readonly DecorRentalDbContext _context;

    public EfKitCategoryRepository(DecorRentalDbContext context)
    {
        _context = context;
    }

    public Task<KitCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.KitCategories
            .Include(category => category.Items)
            .FirstOrDefaultAsync(category => category.Id == id, cancellationToken);

    public async Task<IReadOnlyList<KitCategory>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.KitCategories
            .Include(category => category.Items)
            .AsNoTracking()
            .OrderBy(category => category.Name)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(KitCategory category, CancellationToken cancellationToken = default)
    {
        await _context.KitCategories.AddAsync(category, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveAsync(KitCategory category, CancellationToken cancellationToken = default)
    {
        if (_context.Entry(category).State == EntityState.Detached)
        {
            throw new InvalidOperationException("Category must be tracked before saving changes.");
        }

        _context.ChangeTracker.DetectChanges();

        foreach (var categoryItem in category.Items)
        {
            var categoryItemEntry = _context.Entry(categoryItem);
            if (categoryItemEntry.State == EntityState.Detached)
            {
                _context.Attach(categoryItem);
                categoryItemEntry = _context.Entry(categoryItem);
            }

            if (categoryItemEntry.State is EntityState.Unchanged or EntityState.Modified)
            {
                var categoryItemExists = await _context.Set<CategoryItem>()
                    .AsNoTracking()
                    .AnyAsync(existingCategoryItem => existingCategoryItem.Id == categoryItem.Id, cancellationToken);

                if (!categoryItemExists)
                {
                    categoryItemEntry.State = EntityState.Added;
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
