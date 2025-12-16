using FluentAssertions;
using FitnessReservation.Pricing.Models;
using Xunit;
using FitnessReservation.Pricing.Services;

namespace FitnessReservation.Pricing.Tests;

public class BasePriceProviderTests
{
    [Theory]
    [InlineData(SportType.Yoga, 1250)]
    [InlineData(SportType.Pilates, 1280)]
    [InlineData(SportType.Spinning, 1260)]
    [InlineData(SportType.HIIT, 1270)]
    [InlineData(SportType.Zumba, 1240)]
    public void GetBasePrice_ShouldReturnExpectedPrice(SportType sport, decimal expected)
    {
        // Arrange
        var provider = new BasePriceProvider();

        // Act
        var price = provider.GetBasePrice(sport);

        // Assert
        price.Should().Be(expected);
    }
}
