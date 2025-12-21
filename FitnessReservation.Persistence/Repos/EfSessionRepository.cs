using FitnessReservation.Persistence.Entities;
using FitnessReservation.Reservations.Models;
using FitnessReservation.Reservations.Repos;
using FitnessReservation.Pricing.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessReservation.Persistence.Repos;

public sealed class EfSessionRepository : ISessionRepository
{
    private readonly FitnessReservationDbContext _db;

    public EfSessionRepository(FitnessReservationDbContext db)
        => _db = db;

    public ClassSession? Get(Guid sessionId)
    {
        var s = _db.Sessions.AsNoTracking().FirstOrDefault(x => x.SessionId == sessionId);
        if (s is null) return null;

        return new ClassSession
        {
            SessionId = s.SessionId,
            Sport = s.Sport,
            StartsAtUtc = s.StartsAtUtc,
            Capacity = s.Capacity,
            InstructorName = s.InstructorName
        };
    }

    public IEnumerable<ClassSession> Query(SportType sport, DateTime fromUtc, DateTime toUtc)
    {
        return _db.Sessions.AsNoTracking()
            .Where(s => s.Sport == sport && s.StartsAtUtc >= fromUtc && s.StartsAtUtc < toUtc)
            .OrderBy(s => s.StartsAtUtc)
            .Select(s => new ClassSession
            {
                SessionId = s.SessionId,
                Sport = s.Sport,
                StartsAtUtc = s.StartsAtUtc,
                Capacity = s.Capacity,
                InstructorName = s.InstructorName
            })
            .ToList();
    }
}
