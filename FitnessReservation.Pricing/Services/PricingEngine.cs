using FitnessReservation.Pricing.Models;

namespace FitnessReservation.Pricing.Services;

public sealed class PricingEngine
{
    private readonly BasePriceProvider _basePriceProvider;
    private readonly MultiplierProvider _multiplierProvider;

    public PricingEngine(
        BasePriceProvider basePriceProvider,
        MultiplierProvider multiplierProvider)
    {
        _basePriceProvider = basePriceProvider ?? throw new ArgumentNullException(nameof(basePriceProvider));
        _multiplierProvider = multiplierProvider ?? throw new ArgumentNullException(nameof(multiplierProvider));
    }

    public PricingResult Calculate(PricingRequest request)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));

        var basePrice = _basePriceProvider.GetBasePrice(request.Sport);

        var membershipMultiplier =
            _multiplierProvider.GetMembershipMultiplier(request.Membership);

        var timeMultiplier =
            _multiplierProvider.GetTimeMultiplier(request.IsPeak);

        var occupancyMultiplier =
            _multiplierProvider.GetOccupancyMultiplier(request.Occupancy);

        var totalMultiplier =
            membershipMultiplier * timeMultiplier * occupancyMultiplier;

        var finalPrice = basePrice * totalMultiplier;

        return new PricingResult
        {
            BasePrice = basePrice,
            MembershipMultiplier = membershipMultiplier,
            TimeMultiplier = timeMultiplier,
            OccupancyMultiplier = occupancyMultiplier,
            TotalMultiplier = totalMultiplier,
            FinalPrice = finalPrice
        };
    }
}
