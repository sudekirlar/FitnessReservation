using FitnessReservation.Persistence.Entities;
using FitnessReservation.Reservations.Repos;

namespace FitnessReservation.Persistence.Repos;

public sealed class EfReservationRepository : IReservationRepository
{
    private readonly FitnessReservationDbContext _db;

    public EfReservationRepository(FitnessReservationDbContext db)
        => _db = db;

    public bool Exists(string memberId, Guid sessionId)
    {
        var mid = Guid.Parse(memberId);
        return _db.Reservations.Any(r => r.MemberId == mid && r.SessionId == sessionId);
    }

    public int CountBySession(Guid sessionId)
        => _db.Reservations.Count(r => r.SessionId == sessionId);

    public void Add(string memberId, Guid sessionId, decimal finalPrice, DateTime createdAtUtc)
    {
        var entity = new ReservationEntity
        {
            ReservationId = Guid.NewGuid(),
            MemberId = Guid.Parse(memberId),
            SessionId = sessionId,
            FinalPrice = finalPrice,
            CreatedAtUtc = createdAtUtc
        };

        _db.Reservations.Add(entity);
        _db.SaveChanges();
    }
}
