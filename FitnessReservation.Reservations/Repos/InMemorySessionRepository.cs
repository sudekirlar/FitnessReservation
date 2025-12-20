using FitnessReservation.Pricing.Models;
using FitnessReservation.Reservations.Models;

namespace FitnessReservation.Reservations.Repos;

public sealed class InMemorySessionRepository : ISessionRepository
{
    private readonly Dictionary<Guid, ClassSession> _sessions = new();

    public ClassSession? Get(Guid sessionId)
        => _sessions.TryGetValue(sessionId, out var session) ? session : null;

    public void Upsert(ClassSession session)
        => _sessions[session.SessionId] = session;

    public IEnumerable<ClassSession> Query(SportType sport, DateTime fromUtc, DateTime toUtc)
        => _sessions.Values
            .Where(s => s.Sport == sport &&
                        s.StartsAtUtc >= fromUtc &&
                        s.StartsAtUtc < toUtc)
            .OrderBy(s => s.StartsAtUtc);
}
