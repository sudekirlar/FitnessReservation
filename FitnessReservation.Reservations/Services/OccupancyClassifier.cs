using FitnessReservation.Pricing.Models;

namespace FitnessReservation.Reservations.Services;

public interface IOccupancyClassifier
{
    OccupancyLevel Classify(int reservedCount, int capacity);
}

public sealed class OccupancyClassifier : IOccupancyClassifier
{
    public OccupancyLevel Classify(int reservedCount, int capacity)
    {
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "Capacity must be > 0.");

        if (reservedCount < 0)
            throw new ArgumentOutOfRangeException(nameof(reservedCount), reservedCount, "ReservedCount must be >= 0.");

        var ratio = (decimal)reservedCount / capacity;

        if (ratio < 0.50m) return OccupancyLevel.Low;
        if (ratio < 0.80m) return OccupancyLevel.Mid;
        return OccupancyLevel.High;
    }
}
