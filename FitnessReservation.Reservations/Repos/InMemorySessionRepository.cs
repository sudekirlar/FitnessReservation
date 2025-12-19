using FitnessReservation.Pricing.Models;

namespace FitnessReservation.Reservations.Repos;

public sealed class InMemorySessionRepository : ISessionRepository
{
    public object? Get(Guid sessionId) => null; // empty store, test için yeterli şuanlık.

    public void Upsert(ClassSession classSession)
    {
        throw new NotImplementedException();
    }
}

public class ClassSession
{
    public SportType Sport { get; set; }
    public DateTime StartsAtUtc { get; set; }
    public int Capacity { get; set; }
    public Guid SessionId { get; set; }
}