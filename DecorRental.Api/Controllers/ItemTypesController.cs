using DecorRental.Api.Contracts;
using DecorRental.Api.Security;
using DecorRental.Application.UseCases.CreateItemType;
using DecorRental.Application.UseCases.GetItemTypes;
using DecorRental.Application.UseCases.UpdateItemStock;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DecorRental.Api.Controllers;

[ApiController]
[Route("api/item-types")]
[Authorize(Policy = AuthorizationPolicies.ReadKits)]
public sealed class ItemTypesController : ControllerBase
{
    private readonly CreateItemTypeHandler _createHandler;
    private readonly GetItemTypesHandler _getHandler;
    private readonly UpdateItemStockHandler _updateStockHandler;

    public ItemTypesController(
        CreateItemTypeHandler createHandler,
        GetItemTypesHandler getHandler,
        UpdateItemStockHandler updateStockHandler)
    {
        _createHandler = createHandler;
        _getHandler = getHandler;
        _updateStockHandler = updateStockHandler;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ItemTypeResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _getHandler.HandleAsync(cancellationToken);
        var response = result.Items
            .Select(item => new ItemTypeResponse(item.Id, item.Name, item.TotalStock))
            .ToList();

        return Ok(response);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.ManageKits)]
    [ProducesResponseType(typeof(ItemTypeResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateItemTypeRequest request, CancellationToken cancellationToken)
    {
        var itemTypeId = await _createHandler.HandleAsync(
            new CreateItemTypeCommand(request.Name, request.TotalStock),
            cancellationToken);

        return CreatedAtAction(nameof(GetAll), new { id = itemTypeId }, new ItemTypeResponse(itemTypeId, request.Name, request.TotalStock));
    }

    [HttpPatch("{id:guid}/stock")]
    [Authorize(Policy = AuthorizationPolicies.ManageKits)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateStock(Guid id, [FromBody] UpdateItemStockRequest request, CancellationToken cancellationToken)
    {
        await _updateStockHandler.HandleAsync(new UpdateItemStockCommand(id, request.TotalStock), cancellationToken);
        return NoContent();
    }
}
