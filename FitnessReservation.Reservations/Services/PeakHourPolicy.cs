namespace FitnessReservation.Reservations.Services;

public interface IPeakHourPolicy
{
    bool IsPeak(DateTime startsAtUtc);
}

public sealed class PeakHourPolicy : IPeakHourPolicy
{
    // Peak: UTC ele alınıyor.
    public bool IsPeak(DateTime startsAtUtc)
    {
        var localTime = startsAtUtc.AddHours(3);
        var hour = localTime.Hour;
        return hour >= 18 && hour < 22;
    }
}
