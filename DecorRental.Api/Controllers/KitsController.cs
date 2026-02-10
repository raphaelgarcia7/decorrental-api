using DecorRental.Application.UseCases.ReserveKit;
using Microsoft.AspNetCore.Mvc;

namespace DecorRental.Api.Controllers;

[ApiController]
[Route("api/kits")]
public class KitsController : ControllerBase
{
    private readonly ReserveKitHandler _handler;

    public KitsController(ReserveKitHandler handler)
    {
        _handler = handler;
    }

    [HttpPost("{id}/reservations")]
    public IActionResult Reserve(
        Guid id,
        [FromBody] ReserveKitRequest request)
    {
        var command = new ReserveKitCommand(
            id,
            request.StartDate,
            request.EndDate);

        _handler.Handle(command);

        return NoContent();
    }
}

public record ReserveKitRequest(
    DateOnly StartDate,
    DateOnly EndDate
);
