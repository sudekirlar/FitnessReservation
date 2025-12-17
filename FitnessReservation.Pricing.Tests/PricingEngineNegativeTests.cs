using System;
using FluentAssertions;
using FitnessReservation.Pricing.Services;
using Xunit;

namespace FitnessReservation.Pricing.Tests;

public sealed class PricingEngineNegativeTests
{
    [Fact]
    public void Calculate_NullRequest_ShouldThrowArgumentNullException()
    {
        // Arrange
        var engine = new PricingEngine(new BasePriceProvider(), new MultiplierProvider());

        // Act
        var act = () => engine.Calculate(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("request");
    }

    [Fact]
    public void Ctor_NullBasePriceProvider_ShouldThrowArgumentNullException()
    {
        // Act
        var act = () => new PricingEngine(null!, new MultiplierProvider());

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("basePriceProvider");
    }

    [Fact]
    public void Ctor_NullMultiplierProvider_ShouldThrowArgumentNullException()
    {
        // Act
        var act = () => new PricingEngine(new BasePriceProvider(), null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("multiplierProvider");
    }
}
