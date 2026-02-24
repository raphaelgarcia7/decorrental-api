using DecorRental.Application.Exceptions;
using DecorRental.Domain.Entities;
using DecorRental.Domain.Repositories;

namespace DecorRental.Application.UseCases.AddCategoryItem;

public sealed class AddCategoryItemHandler
{
    private readonly IKitCategoryRepository _categoryRepository;
    private readonly IItemTypeRepository _itemTypeRepository;

    public AddCategoryItemHandler(IKitCategoryRepository categoryRepository, IItemTypeRepository itemTypeRepository)
    {
        _categoryRepository = categoryRepository;
        _itemTypeRepository = itemTypeRepository;
    }

    public async Task<KitCategory> HandleAsync(AddCategoryItemCommand command, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(command.CategoryId, cancellationToken)
            ?? throw new NotFoundException("Categoria nao encontrada.");

        var itemType = await _itemTypeRepository.GetByIdAsync(command.ItemTypeId, cancellationToken)
            ?? throw new NotFoundException("Tipo de item nao encontrado.");

        _ = itemType;
        category.AddOrUpdateItem(command.ItemTypeId, command.Quantity);
        await _categoryRepository.SaveAsync(category, cancellationToken);

        return category;
    }
}
