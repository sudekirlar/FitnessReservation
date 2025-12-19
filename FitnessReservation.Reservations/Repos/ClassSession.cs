using FitnessReservation.Pricing.Models;

namespace FitnessReservation.Reservations.Models;

public sealed class ClassSession
{
    public Guid SessionId { get; init; }
    public SportType Sport { get; init; }
    public DateTime StartsAtUtc { get; init; }
    public int Capacity { get; init; }
}
