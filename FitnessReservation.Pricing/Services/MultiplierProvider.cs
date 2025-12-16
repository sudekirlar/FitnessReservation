using FitnessReservation.Pricing.Models;

namespace FitnessReservation.Pricing.Services;

public sealed class MultiplierProvider
{
    public decimal GetMembershipMultiplier(MembershipType membershipType)
    {
        throw new NotImplementedException();
    }

    public decimal GetTimeMultiplier(bool isPeakHour)
    {
        throw new NotImplementedException();
    }

    public decimal GetOccupancyMultiplier(OccupancyLevel occupancyLevel)
    {
        throw new NotImplementedException();
    }
}
