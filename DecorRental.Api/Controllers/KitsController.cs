using DecorRental.Api.Contracts;
using DecorRental.Application.UseCases.CancelReservation;
using DecorRental.Application.UseCases.CreateKit;
using DecorRental.Application.UseCases.GetKitById;
using DecorRental.Application.UseCases.GetKits;
using DecorRental.Application.UseCases.ReserveKit;
using DecorRental.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace DecorRental.Api.Controllers;

[ApiController]
[Route("api/kits")]
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
    [ProducesResponseType(typeof(KitSummaryResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateKitRequest request, CancellationToken cancellationToken)
    {
        var kitId = await _createHandler.HandleAsync(new CreateKitCommand(request.Name), cancellationToken);
        var response = new KitSummaryResponse(kitId, request.Name);

        return CreatedAtAction(nameof(GetById), new { id = kitId }, response);
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
        var kit = await _getKitByIdHandler.HandleAsync(new GetKitByIdQuery(id), cancellationToken);
        var response = ToDetail(kit);

        return Ok(response);
    }

    [HttpGet("{id:guid}/reservations")]
    [ProducesResponseType(typeof(IReadOnlyList<ReservationResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReservations(Guid id, CancellationToken cancellationToken)
    {
        var kit = await _getKitByIdHandler.HandleAsync(new GetKitByIdQuery(id), cancellationToken);
        var response = kit.Reservations
            .Select(reservation => new ReservationResponse(
                reservation.Id,
                reservation.Period.Start,
                reservation.Period.End,
                reservation.Status.ToString()))
            .ToList();

        return Ok(response);
    }

    [HttpPost("{id:guid}/reservations")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Reserve(Guid id, [FromBody] ReserveKitRequest request, CancellationToken cancellationToken)
    {
        var command = new ReserveKitCommand(
            id,
            request.StartDate,
            request.EndDate);

        await _reserveHandler.HandleAsync(command, cancellationToken);

        return NoContent();
    }

    [HttpPost("{id:guid}/reservations/{reservationId:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Cancel(Guid id, Guid reservationId, CancellationToken cancellationToken)
    {
        await _cancelHandler.HandleAsync(new CancelReservationCommand(id, reservationId), cancellationToken);
        return NoContent();
    }

    private static KitSummaryResponse ToSummary(Kit kit)
        => new(kit.Id, kit.Name);

    private static KitDetailResponse ToDetail(Kit kit)
    {
        var reservations = kit.Reservations
            .Select(reservation => new ReservationResponse(
                reservation.Id,
                reservation.Period.Start,
                reservation.Period.End,
                reservation.Status.ToString()))
            .ToList();

        return new KitDetailResponse(kit.Id, kit.Name, reservations);
    }
}
