using DecorRental.Api.Contracts;
using DecorRental.Api.Security;
using DecorRental.Application.UseCases.CancelReservation;
using DecorRental.Application.UseCases.CreateKit;
using DecorRental.Application.UseCases.GetKitById;
using DecorRental.Application.UseCases.GetKits;
using DecorRental.Application.UseCases.ReserveKit;
using DecorRental.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DecorRental.Api.Controllers;

[ApiController]
[Route("api/kits")]
[Authorize(Policy = AuthorizationPolicies.ReadKits)]
public class KitsController : ControllerBase
{
    private readonly CreateKitHandler _createHandler;
    private readonly GetKitsHandler _getKitsHandler;
    private readonly GetKitByIdHandler _getKitByIdHandler;
    private readonly ReserveKitHandler _reserveHandler;
    private readonly CancelReservationHandler _cancelHandler;

    public KitsController(
        CreateKitHandler createHandler,
        GetKitsHandler getKitsHandler,
        GetKitByIdHandler getKitByIdHandler,
        ReserveKitHandler reserveHandler,
        CancelReservationHandler cancelHandler)
    {
        _createHandler = createHandler;
        _getKitsHandler = getKitsHandler;
        _getKitByIdHandler = getKitByIdHandler;
        _reserveHandler = reserveHandler;
        _cancelHandler = cancelHandler;
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.ManageKits)]
    [ProducesResponseType(typeof(KitSummaryResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateKitRequest request, CancellationToken cancellationToken)
    {
        var kitThemeId = await _createHandler.HandleAsync(new CreateKitCommand(request.Name), cancellationToken);
        var response = new KitSummaryResponse(kitThemeId, request.Name);

        return CreatedAtAction(nameof(GetById), new { id = kitThemeId }, response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<KitSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] GetKitsRequest request, CancellationToken cancellationToken)
    {
        var result = await _getKitsHandler.HandleAsync(new GetKitsQuery(request.Page, request.PageSize), cancellationToken);
        var response = new PagedResponse<KitSummaryResponse>(
            result.Items.Select(ToSummary).ToList(),
            result.Page,
            result.PageSize,
            result.TotalCount);

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(KitDetailResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var kitTheme = await _getKitByIdHandler.HandleAsync(new GetKitByIdQuery(id), cancellationToken);
        var response = ToDetail(kitTheme);

        return Ok(response);
    }

    [HttpGet("{id:guid}/reservations")]
    [ProducesResponseType(typeof(IReadOnlyList<ReservationResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReservations(Guid id, CancellationToken cancellationToken)
    {
        var kitTheme = await _getKitByIdHandler.HandleAsync(new GetKitByIdQuery(id), cancellationToken);
        var response = kitTheme.Reservations
            .Select(reservation => new ReservationResponse(
                reservation.Id,
                reservation.KitCategoryId,
                reservation.Period.Start,
                reservation.Period.End,
                reservation.Status.ToString(),
                reservation.IsStockOverride,
                reservation.StockOverrideReason))
            .ToList();

        return Ok(response);
    }

    [HttpPost("{id:guid}/reservations")]
    [Authorize(Policy = AuthorizationPolicies.ManageKits)]
    [ProducesResponseType(typeof(ReserveKitResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Reserve(Guid id, [FromBody] ReserveKitRequest request, CancellationToken cancellationToken)
    {
        var command = new ReserveKitCommand(
            id,
            request.KitCategoryId,
            request.StartDate,
            request.EndDate,
            request.AllowStockOverride,
            request.StockOverrideReason);

        var result = await _reserveHandler.HandleAsync(command, cancellationToken);
        var response = new ReserveKitResponse(
            result.ReservationId,
            result.KitThemeId,
            result.KitCategoryId,
            result.StartDate,
            result.EndDate,
            result.ReservationStatus,
            result.IsStockOverride,
            result.StockOverrideReason,
            "Reservation created successfully.");

        return Ok(response);
    }

    [HttpPost("{id:guid}/reservations/{reservationId:guid}/cancel")]
    [Authorize(Policy = AuthorizationPolicies.ManageKits)]
    [ProducesResponseType(typeof(CancelReservationResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Cancel(Guid id, Guid reservationId, CancellationToken cancellationToken)
    {
        var result = await _cancelHandler.HandleAsync(new CancelReservationCommand(id, reservationId), cancellationToken);
        var response = new CancelReservationResponse(
            result.ReservationId,
            result.KitThemeId,
            result.ReservationStatus,
            "Reservation cancelled successfully.");

        return Ok(response);
    }

    private static KitSummaryResponse ToSummary(KitTheme kitTheme)
        => new(kitTheme.Id, kitTheme.Name);

    private static KitDetailResponse ToDetail(KitTheme kitTheme)
    {
        var reservations = kitTheme.Reservations
            .Select(reservation => new ReservationResponse(
                reservation.Id,
                reservation.KitCategoryId,
                reservation.Period.Start,
                reservation.Period.End,
                reservation.Status.ToString(),
                reservation.IsStockOverride,
                reservation.StockOverrideReason))
            .ToList();

        return new KitDetailResponse(kitTheme.Id, kitTheme.Name, reservations);
    }
}
