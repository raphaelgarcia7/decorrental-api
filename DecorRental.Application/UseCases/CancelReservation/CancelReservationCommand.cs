namespace DecorRental.Application.UseCases.CancelReservation;

public record CancelReservationCommand(Guid KitThemeId, Guid ReservationId);
