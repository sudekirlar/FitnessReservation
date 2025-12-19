using System;
using FitnessReservation.Pricing.Models;
using FitnessReservation.Pricing.Services;
using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;

namespace FitnessReservation.Pricing.Tests;

public class PricingPropertyTests
{
    private static PricingEngine Engine()
        => new PricingEngine(new BasePriceProvider(), new MultiplierProvider());

    public static Arbitrary<PricingRequest> ValidRequests()
    {
        var gen =
            from sport in Gen.Elements(Enum.GetValues<SportType>())
            from membership in Gen.Elements(Enum.GetValues<MembershipType>())
            from isPeak in Gen.Elements(false, true)
            from occupancy in Gen.Elements(Enum.GetValues<OccupancyLevel>())
            select new PricingRequest
            {
                Sport = sport,
                Membership = membership,
                IsPeak = isPeak,
                Occupancy = occupancy
            };

        return Arb.From(gen);
    }

    private static PricingRequest Make(PricingRequest r, OccupancyLevel occ)
        => new PricingRequest
        {
            Sport = r.Sport,
            Membership = r.Membership,
            IsPeak = r.IsPeak,
            Occupancy = occ
        };

    private static PricingRequest Make(PricingRequest r, bool isPeak)
        => new PricingRequest
        {
            Sport = r.Sport,
            Membership = r.Membership,
            IsPeak = isPeak,
            Occupancy = r.Occupancy
        };

    [Property(Arbitrary = new[] { typeof(PricingPropertyTests) }, MaxTest = 200)]
    public bool Price_should_not_decrease_when_occupancy_increases(PricingRequest request)
    {
        var engine = Engine();

        var low = engine.Calculate(Make(request, OccupancyLevel.Low)).FinalPrice;
        var mid = engine.Calculate(Make(request, OccupancyLevel.Mid)).FinalPrice;
        var high = engine.Calculate(Make(request, OccupancyLevel.High)).FinalPrice;

        return low <= mid && mid <= high;
    }

    [Property(Arbitrary = new[] { typeof(PricingPropertyTests) }, MaxTest = 200)]
    public bool Price_should_not_decrease_from_offpeak_to_peak(PricingRequest request)
    {
        var engine = Engine();

        var offPeak = engine.Calculate(Make(request, isPeak: false)).FinalPrice;
        var peak = engine.Calculate(Make(request, isPeak: true)).FinalPrice;

        return offPeak <= peak;
    }
}
