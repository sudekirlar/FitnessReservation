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

        var hour = startsAtUtc.Hour; // 0..23
        return hour >= 18 && hour < 22;
    }
}
