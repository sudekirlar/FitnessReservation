namespace FitnessReservation.Pricing.Models;

public sealed class PricingResult
{
    public decimal BasePrice { get; init; }

    public decimal MembershipMultiplier { get; init; }
    public decimal TimeMultiplier { get; init; }
    public decimal OccupancyMultiplier { get; init; }

    public decimal TotalMultiplier { get; init; }
    public decimal FinalPrice { get; init; }
}
