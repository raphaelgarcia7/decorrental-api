namespace DecorRental.Application.UseCases.CancelReservation;

public record CancelReservationCommand(Guid KitId, Guid ReservationId);
