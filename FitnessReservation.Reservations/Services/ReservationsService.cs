using FitnessReservation.Pricing.Services;
using FitnessReservation.Reservations.Models;
using FitnessReservation.Reservations.Repos;

namespace FitnessReservation.Reservations.Services;

public sealed class ReservationsService
{
    private readonly ISessionRepository _sessions;

    public ReservationsService(
        ISessionRepository sessions,
        IReservationRepository reservations,
        PricingEngine pricing,
        IClock clock)
    {
        _sessions = sessions ?? throw new ArgumentNullException(nameof(sessions));
    }

    public ReserveResult Reserve(ReserveRequest request)
    {
        var session = _sessions.Get(request.SessionId);
        if (session is null) return ReserveResult.Fail(ReserveError.SessionNotFound);

        throw new NotImplementedException(); 
    }
}
