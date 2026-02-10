using DecorRental.Domain.Entities;

namespace DecorRental.Domain.Repositories;

public interface IKitRepository
{
    Kit? GetById(Guid id);
    IReadOnlyList<Kit> GetAll();
    void Add(Kit kit);
    void Save(Kit kit);
}
