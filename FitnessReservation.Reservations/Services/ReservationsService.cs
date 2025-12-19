using FitnessReservation.Pricing.Models;
using FitnessReservation.Pricing.Services;
using FitnessReservation.Reservations.Models;
using FitnessReservation.Reservations.Repos;

namespace FitnessReservation.Reservations.Services;

public sealed class ReservationsService
{
    private readonly ISessionRepository _sessions;
    private readonly IReservationRepository _reservations;
    private readonly PricingEngine _pricing;
    private readonly IClock _clock;

    public ReservationsService(
        ISessionRepository sessions,
        IReservationRepository reservations,
        PricingEngine pricing,
        IClock clock)
    {
        _sessions = sessions ?? throw new ArgumentNullException(nameof(sessions));
        _reservations = reservations ?? throw new ArgumentNullException(nameof(reservations));
        _pricing = pricing ?? throw new ArgumentNullException(nameof(pricing));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }
    public ReserveResult Reserve(ReserveRequest request)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));

        var session = _sessions.Get(request.SessionId);
        if (session is null)
            return ReserveResult.Fail(ReserveError.SessionNotFound);

        if (session.StartsAtUtc <= _clock.UtcNow)
            return ReserveResult.Fail(ReserveError.SessionInPast);

        // duplicate check
        if (_reservations.Exists(request.MemberId, request.SessionId))
            return ReserveResult.Fail(ReserveError.DuplicateReservation);

        var price = _pricing.Calculate(new PricingRequest
        {
            Sport = session.Sport,
            Membership = request.Membership,
            IsPeak = false,
            Occupancy = OccupancyLevel.Low
        });

        _reservations.Add(request.MemberId, request.SessionId);

        return ReserveResult.Ok(Guid.NewGuid(), price);
    }

}
