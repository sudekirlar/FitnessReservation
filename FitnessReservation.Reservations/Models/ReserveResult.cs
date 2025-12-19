using FitnessReservation.Pricing.Models;

namespace FitnessReservation.Reservations.Models;

public sealed class ReserveResult
{
    public bool Success { get; init; }
    public ReserveError Error { get; init; }

    public Guid? ReservationId { get; init; }
    public PricingResult? PriceSnapshot { get; init; }

    public static ReserveResult Fail(ReserveError error) => new()
    {
        Success = false,
        Error = error
    };

    public static ReserveResult Ok(Guid reservationId, PricingResult snapshot) => new()
    {
        Success = true,
        Error = ReserveError.None,
        ReservationId = reservationId,
        PriceSnapshot = snapshot
    };
}
