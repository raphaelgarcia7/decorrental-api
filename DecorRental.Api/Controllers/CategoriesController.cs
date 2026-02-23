using DecorRental.Api.Contracts;
using DecorRental.Api.Security;
using DecorRental.Application.UseCases.AddCategoryItem;
using DecorRental.Application.UseCases.CreateKitCategory;
using DecorRental.Application.UseCases.GetKitCategoryById;
using DecorRental.Application.UseCases.GetKitCategories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DecorRental.Api.Controllers;

[ApiController]
[Route("api/categories")]
[Authorize(Policy = AuthorizationPolicies.ReadKits)]
public sealed class CategoriesController : ControllerBase
{
    private readonly CreateKitCategoryHandler _createHandler;
    private readonly AddCategoryItemHandler _addCategoryItemHandler;
    private readonly GetKitCategoryByIdHandler _getByIdHandler;
    private readonly GetKitCategoriesHandler _getHandler;

    public CategoriesController(
        CreateKitCategoryHandler createHandler,
        AddCategoryItemHandler addCategoryItemHandler,
        GetKitCategoryByIdHandler getByIdHandler,
        GetKitCategoriesHandler getHandler)
    {
        _createHandler = createHandler;
        _addCategoryItemHandler = addCategoryItemHandler;
        _getByIdHandler = getByIdHandler;
        _getHandler = getHandler;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CategoryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _getHandler.HandleAsync(cancellationToken);
        var response = result.Items
            .Select(ToResponse)
            .ToList();

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var category = await _getByIdHandler.HandleAsync(id, cancellationToken);
        return Ok(ToResponse(category));
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.ManageKits)]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var category = await _createHandler.HandleAsync(new CreateKitCategoryCommand(request.Name), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = category.Id }, ToResponse(category));
    }

    [HttpPost("{id:guid}/items")]
    [Authorize(Policy = AuthorizationPolicies.ManageKits)]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddItem(Guid id, [FromBody] AddCategoryItemRequest request, CancellationToken cancellationToken)
    {
        var command = new AddCategoryItemCommand(id, request.ItemTypeId, request.Quantity);
        var category = await _addCategoryItemHandler.HandleAsync(command, cancellationToken);

        return Ok(ToResponse(category));
    }

    private static CategoryResponse ToResponse(Domain.Entities.KitCategory category)
    {
        return new CategoryResponse(
            category.Id,
            category.Name,
            category.Items
                .Select(categoryItem => new CategoryItemResponse(categoryItem.ItemTypeId, categoryItem.Quantity))
                .ToList());
    }
}
