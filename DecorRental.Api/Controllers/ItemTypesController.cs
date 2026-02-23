using DecorRental.Api.Contracts;
using DecorRental.Api.Security;
using DecorRental.Application.UseCases.CreateItemType;
using DecorRental.Application.UseCases.GetItemTypeById;
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
    private readonly GetItemTypeByIdHandler _getByIdHandler;
    private readonly GetItemTypesHandler _getHandler;
    private readonly UpdateItemStockHandler _updateStockHandler;

    public ItemTypesController(
        CreateItemTypeHandler createHandler,
        GetItemTypeByIdHandler getByIdHandler,
        GetItemTypesHandler getHandler,
        UpdateItemStockHandler updateStockHandler)
    {
        _createHandler = createHandler;
        _getByIdHandler = getByIdHandler;
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

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ItemTypeResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var itemType = await _getByIdHandler.HandleAsync(id, cancellationToken);
        return Ok(ToResponse(itemType.Id, itemType.Name, itemType.TotalStock));
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.ManageKits)]
    [ProducesResponseType(typeof(ItemTypeResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateItemTypeRequest request, CancellationToken cancellationToken)
    {
        var itemType = await _createHandler.HandleAsync(
            new CreateItemTypeCommand(request.Name, request.TotalStock),
            cancellationToken);

        var response = ToResponse(itemType.Id, itemType.Name, itemType.TotalStock);
        return CreatedAtAction(nameof(GetById), new { id = itemType.Id }, response);
    }

    [HttpPatch("{id:guid}/stock")]
    [Authorize(Policy = AuthorizationPolicies.ManageKits)]
    [ProducesResponseType(typeof(ItemTypeResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateStock(Guid id, [FromBody] UpdateItemStockRequest request, CancellationToken cancellationToken)
    {
        var itemType = await _updateStockHandler.HandleAsync(new UpdateItemStockCommand(id, request.TotalStock), cancellationToken);
        return Ok(ToResponse(itemType.Id, itemType.Name, itemType.TotalStock));
    }

    private static ItemTypeResponse ToResponse(Guid id, string name, int totalStock)
        => new(id, name, totalStock);
}
