using FitnessReservation.Pricing.Models;

namespace FitnessReservation.Pricing.Services;

public sealed class PricingEngine
{
    private BasePriceProvider basePriceProvider;
    private MultiplierProvider multiplierProvider;

    public PricingEngine(BasePriceProvider basePriceProvider, MultiplierProvider multiplierProvider)
    {
        this.basePriceProvider = basePriceProvider;
        this.multiplierProvider = multiplierProvider;
    }

    public decimal BasePriceProvider { get; init; }
    public decimal MultiplierProvider { get; init; }
    public PricingResult Calculate(PricingRequest request)
    {
        throw new NotImplementedException();
    }
}
