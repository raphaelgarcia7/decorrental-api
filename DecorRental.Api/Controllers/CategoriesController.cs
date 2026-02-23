using DecorRental.Api.Contracts;
using DecorRental.Api.Security;
using DecorRental.Application.UseCases.AddCategoryItem;
using DecorRental.Application.UseCases.CreateKitCategory;
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
    private readonly GetKitCategoriesHandler _getHandler;

    public CategoriesController(
        CreateKitCategoryHandler createHandler,
        AddCategoryItemHandler addCategoryItemHandler,
        GetKitCategoriesHandler getHandler)
    {
        _createHandler = createHandler;
        _addCategoryItemHandler = addCategoryItemHandler;
        _getHandler = getHandler;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CategoryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _getHandler.HandleAsync(cancellationToken);
        var response = result.Items
            .Select(category => new CategoryResponse(
                category.Id,
                category.Name,
                category.Items
                    .Select(categoryItem => new CategoryItemResponse(categoryItem.ItemTypeId, categoryItem.Quantity))
                    .ToList()))
            .ToList();

        return Ok(response);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.ManageKits)]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var categoryId = await _createHandler.HandleAsync(new CreateKitCategoryCommand(request.Name), cancellationToken);
        return CreatedAtAction(nameof(GetAll), new { id = categoryId }, new CategoryResponse(categoryId, request.Name, []));
    }

    [HttpPost("{id:guid}/items")]
    [Authorize(Policy = AuthorizationPolicies.ManageKits)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AddItem(Guid id, [FromBody] AddCategoryItemRequest request, CancellationToken cancellationToken)
    {
        var command = new AddCategoryItemCommand(id, request.ItemTypeId, request.Quantity);
        await _addCategoryItemHandler.HandleAsync(command, cancellationToken);

        return NoContent();
    }
}
