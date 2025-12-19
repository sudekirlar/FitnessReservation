namespace FitnessReservation.Reservations.Repos;

public sealed class InMemoryReservationRepository : IReservationRepository
{
    private readonly object _gate = new();
    private readonly HashSet<(string MemberId, Guid SessionId)> _keys = new();

    public bool Exists(string memberId, Guid sessionId)
    {
        lock (_gate)
            return _keys.Contains((memberId, sessionId));
    }

    public void Add(string memberId, Guid sessionId)
    {
        lock (_gate)
        {
            if (!_keys.Add((memberId, sessionId)))
                throw new InvalidOperationException("Duplicate reservation detected.");
        }
    }
}
