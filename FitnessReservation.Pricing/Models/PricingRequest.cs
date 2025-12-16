using FitnessReservation.Pricing.Models;

namespace FitnessReservation.Pricing.Models;

public sealed class PricingRequest
{
    public SportType Sport { get; init; }
    public MembershipType Membership { get; init; }
    public bool IsPeak { get; init; }
    public OccupancyLevel Occupancy { get; init; }
}
