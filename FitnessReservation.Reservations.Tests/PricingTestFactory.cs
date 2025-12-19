using FitnessReservation.Pricing.Services;

namespace FitnessReservation.Reservations.Tests;

internal static class PricingTestFactory
{
    public static PricingEngine Engine()
        => new PricingEngine(new BasePriceProvider(), new MultiplierProvider());
}
