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
    private readonly IPeakHourPolicy _peak;
    private readonly IOccupancyClassifier _occupancy;

    // Rezervasyon işlemini atomik hale getirmek için kullanılan kilit nesnesi.
    // 'static' olması önemlidir; çünkü ReservationsService 'Scoped' olarak kaydedilmiştir.
    // Static yapı, tüm HTTP isteklerinin aynı kilit nesnesine tabi olmasını sağlar.
    private static readonly object _reservationLock = new();

    public ReservationsService(
        ISessionRepository sessions,
        IReservationRepository reservations,
        PricingEngine pricing,
        IClock clock,
        IPeakHourPolicy peak,
        IOccupancyClassifier occupancy)
    {
        _sessions = sessions ?? throw new ArgumentNullException(nameof(sessions));
        _reservations = reservations ?? throw new ArgumentNullException(nameof(reservations));
        _pricing = pricing ?? throw new ArgumentNullException(nameof(pricing));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _peak = peak ?? throw new ArgumentNullException(nameof(peak));
        _occupancy = occupancy ?? throw new ArgumentNullException(nameof(occupancy));
    }

    public ReserveResult Reserve(ReserveRequest request)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));

        // Bu blok içindeki işlemler bitmeden başka bir thread (istek) içeri giremez.
        lock (_reservationLock)
        {
            var session = _sessions.Get(request.SessionId);
            if (session is null)
                return ReserveResult.Fail(ReserveError.SessionNotFound);

            if (session.StartsAtUtc <= _clock.UtcNow)
                return ReserveResult.Fail(ReserveError.SessionInPast);

            if (_reservations.Exists(request.MemberId, request.SessionId))
                return ReserveResult.Fail(ReserveError.DuplicateReservation);

            var reservedCount = _reservations.CountBySession(request.SessionId);
            if (reservedCount >= session.Capacity)
                return ReserveResult.Fail(ReserveError.CapacityFull);

            // Dinamik fiyatlandırma hesaplamaları
            var isPeak = _peak.IsPeak(session.StartsAtUtc);
            var occ = _occupancy.Classify(reservedCount, session.Capacity);

            var price = _pricing.Calculate(new PricingRequest
            {
                Sport = session.Sport,
                Membership = request.Membership,
                IsPeak = isPeak,
                Occupancy = occ
            });

            _reservations.Add(request.MemberId, request.SessionId, price.FinalPrice, _clock.UtcNow);

            return ReserveResult.Ok(Guid.NewGuid(), price);
        }
    }
}