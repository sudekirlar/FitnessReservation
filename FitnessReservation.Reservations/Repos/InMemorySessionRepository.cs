namespace FitnessReservation.Reservations.Repos;

public sealed class InMemorySessionRepository : ISessionRepository
{
    public object? Get(Guid sessionId) => null; // empty store, test için yeterli şuanlık.
}
