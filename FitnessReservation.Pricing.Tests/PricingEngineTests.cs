using FluentAssertions;
using FitnessReservation.Pricing.Models;
using FitnessReservation.Pricing.Services;
using Xunit;

namespace FitnessReservation.Pricing.Tests;

public class PricingEngineTests
{
    [Fact]
    public void Calculate_ShouldReturnCorrectFinalPrice_ForSimpleCase()
    {
        // Arrange
        var engine = new PricingEngine(
            new BasePriceProvider(),
            new MultiplierProvider()
        );

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
        result.FinalPrice.Should().Be(1250m);
        result.TotalMultiplier.Should().Be(1.0m);
        result.BasePrice.Should().Be(1250m);
    }
}
