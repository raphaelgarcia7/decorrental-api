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
    public IActionResult Create([FromBody] CreateKitRequest request)
    {
        var kitId = _createHandler.Handle(new CreateKitCommand(request.Name));
        var response = new KitSummaryResponse(kitId, request.Name);

        return CreatedAtAction(nameof(GetById), new { id = kitId }, response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<KitSummaryResponse>), StatusCodes.Status200OK)]
    public IActionResult GetAll([FromQuery] GetKitsRequest request)
    {
        var result = _getKitsHandler.Handle(new GetKitsQuery(request.Page, request.PageSize));
        var response = new PagedResponse<KitSummaryResponse>(
            result.Items.Select(ToSummary).ToList(),
            result.Page,
            result.PageSize,
            result.TotalCount);

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(KitDetailResponse), StatusCodes.Status200OK)]
    public IActionResult GetById(Guid id)
    {
        var kit = _getKitByIdHandler.Handle(new GetKitByIdQuery(id));
        var response = ToDetail(kit);

        return Ok(response);
    }

    [HttpGet("{id:guid}/reservations")]
    [ProducesResponseType(typeof(IReadOnlyList<ReservationResponse>), StatusCodes.Status200OK)]
    public IActionResult GetReservations(Guid id)
    {
        var kit = _getKitByIdHandler.Handle(new GetKitByIdQuery(id));
        var response = kit.Reservations
            .Select(r => new ReservationResponse(
                r.Id,
                r.Period.Start,
                r.Period.End,
                r.Status.ToString()))
            .ToList();

        return Ok(response);
    }

    [HttpPost("{id:guid}/reservations")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult Reserve(Guid id, [FromBody] ReserveKitRequest request)
    {
        var command = new ReserveKitCommand(
            id,
            request.StartDate,
            request.EndDate);

        _reserveHandler.Handle(command);

        return NoContent();
    }

    [HttpPost("{id:guid}/reservations/{reservationId:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult Cancel(Guid id, Guid reservationId)
    {
        _cancelHandler.Handle(new CancelReservationCommand(id, reservationId));
        return NoContent();
    }

    private static KitSummaryResponse ToSummary(Kit kit)
        => new(kit.Id, kit.Name);

    private static KitDetailResponse ToDetail(Kit kit)
    {
        var reservations = kit.Reservations
            .Select(r => new ReservationResponse(
                r.Id,
                r.Period.Start,
                r.Period.End,
                r.Status.ToString()))
            .ToList();

        return new KitDetailResponse(kit.Id, kit.Name, reservations);
    }
}
