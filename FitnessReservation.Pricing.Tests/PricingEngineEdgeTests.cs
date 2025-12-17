using FluentAssertions;
using FitnessReservation.Pricing.Models;
using FitnessReservation.Pricing.Services;
using Xunit;

namespace FitnessReservation.Pricing.Tests;

public sealed class PricingEngine_EdgeTests
{
    private static PricingEngine CreateEngine()
        => new PricingEngine(new BasePriceProvider(), new MultiplierProvider());

    // The tests given are taken from decision table we made.
    [Fact]
    public void Calculate_ShouldReturnBasePrice_WhenAllMultipliersAreOne()
    {
        // Arrange
        var engine = CreateEngine();

        var request = new PricingRequest
        {
            Sport = SportType.Yoga,
            Membership = MembershipType.Standard,
            IsPeak = false,
            Occupancy = OccupancyLevel.Low
        };

        // Act
        var result = engine.Calculate(request);

        // Assert
        result.TotalMultiplier.Should().Be(1.0m);
        result.FinalPrice.Should().Be(1250m);
    }

    [Fact]
    public void Calculate_ShouldBeConsistent_WithBreakdown()
    {
        // Arrange
        var engine = CreateEngine();

        // Act
        var request = new PricingRequest
        {
            Sport = SportType.Pilates,
            Membership = MembershipType.Premium,
            IsPeak = true,
            Occupancy = OccupancyLevel.Mid
        };

        var result = engine.Calculate(request);

        // Assert
        result.TotalMultiplier.Should().Be(
            result.MembershipMultiplier * result.TimeMultiplier * result.OccupancyMultiplier);

        result.FinalPrice.Should().Be(
            result.BasePrice * result.TotalMultiplier);
    }

    [Fact]
    public void Calculate_ShouldReturnMinimumPrice_ForCheapestSportAndLowestMultipliers()
    {
        // Arrange
        var engine = CreateEngine();

        var request = new PricingRequest
        {
            Sport = SportType.Zumba,
            Membership = MembershipType.Student,
            IsPeak = false,
            Occupancy = OccupancyLevel.Low
        };

        // Act
        var result = engine.Calculate(request);

        // Assert
        result.FinalPrice.Should().Be(1054m);
    }

    [Fact]
    public void Calculate_ShouldReturnMaximumPrice_ForMostExpensiveSportAndHighestMultipliers()
    {
        // Arrange
        var engine = CreateEngine();

        var request = new PricingRequest
        {
            Sport = SportType.Pilates,
            Membership = MembershipType.Premium,
            IsPeak = true,
            Occupancy = OccupancyLevel.High
        };

        // Act
        var result = engine.Calculate(request);

        // Assert
        result.FinalPrice.Should().Be(2304m);
    }
}
