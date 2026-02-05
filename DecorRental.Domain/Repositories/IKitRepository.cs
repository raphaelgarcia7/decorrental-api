using DecorRental.Domain.Entities;

namespace DecorRental.Domain.Repositories;

public interface IKitRepository
{
    Kit? GetById(Guid id);
    void Save(Kit kit);
}
