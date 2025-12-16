using FitnessReservation.Pricing.Models;

namespace FitnessReservation.Pricing.Services;

public sealed class MultiplierProvider
{
    public decimal GetMembershipMultiplier(MembershipType membershipType)
    {
        return membershipType switch
        {
            MembershipType.Student => 0.85m,
            MembershipType.Standard => 1.0m,
            MembershipType.Premium => 1.2m,
            _ => throw new ArgumentOutOfRangeException(nameof(membershipType), membershipType, "Unknown membership type.")
        };
    }

    public decimal GetTimeMultiplier(bool isPeakHour)
    {
        return isPeakHour ? 1.2m : 1.0m;
    }

    public decimal GetOccupancyMultiplier(OccupancyLevel occupancyLevel)
    {
        return occupancyLevel switch
        {
            OccupancyLevel.Low => 1.0m,
            OccupancyLevel.Mid => 1.1m,
            OccupancyLevel.High => 1.25m,
            _ => throw new ArgumentOutOfRangeException(nameof(occupancyLevel), occupancyLevel, "Unknown occupancy level.")
        };
    }
}
