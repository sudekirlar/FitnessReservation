using FitnessReservation.Reservations.Services;

namespace FitnessReservation.Reservations.Tests;

internal sealed class FakeClock : IClock
{
    public FakeClock(DateTime utcNow) => UtcNow = utcNow;
    public DateTime UtcNow { get; }
}
