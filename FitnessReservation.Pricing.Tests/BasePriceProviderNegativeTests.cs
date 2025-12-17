using System;
using FluentAssertions;
using FitnessReservation.Pricing.Models;
using FitnessReservation.Pricing.Services;
using Xunit;

namespace FitnessReservation.Pricing.Tests;

public sealed class BasePriceProviderNegativeTests
{
    [Fact]
    public void GetBasePrice_UnknownSport_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var provider = new BasePriceProvider();
        var unknownSport = (SportType)999;

        // Act
        var act = () => provider.GetBasePrice(unknownSport);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("sport");
    }
}
