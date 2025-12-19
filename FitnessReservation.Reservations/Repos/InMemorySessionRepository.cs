using FitnessReservation.Reservations.Models;

namespace FitnessReservation.Reservations.Repos;

public sealed class InMemorySessionRepository : ISessionRepository
{
    private readonly Dictionary<Guid, ClassSession> _sessions = new();

    public ClassSession? Get(Guid sessionId)
        => _sessions.TryGetValue(sessionId, out var session) ? session : null;

    public void Upsert(ClassSession session)
        => _sessions[session.SessionId] = session;
}
