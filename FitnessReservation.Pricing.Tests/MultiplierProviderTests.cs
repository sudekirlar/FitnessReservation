using FluentAssertions;
using FitnessReservation.Pricing.Models;
using FitnessReservation.Pricing.Services;
using Xunit;

namespace FitnessReservation.Pricing.Tests;

public class MultiplierProviderTests
{
    [Theory]
    [InlineData(MembershipType.Student, 0.85)]
    [InlineData(MembershipType.Standard, 1.0)]
    [InlineData(MembershipType.Premium, 1.2)]
    public void GetMembershipMultiplier_ShouldReturnExpectedMultiplier(
        MembershipType membershipType,
        decimal expectedMultiplier)
    {
        // Arrange
        var provider = new MultiplierProvider();

        // Act
        var multiplier = provider.GetMembershipMultiplier(membershipType);

        // Assert
        multiplier.Should().Be(expectedMultiplier);
    }

    [Theory]
    [InlineData(false, 1.0)]
    [InlineData(true, 1.2)] 
    public void GetTimeMultiplier_ShouldReturnExpectedMultiplier(
        bool isPeakHour,
        decimal expectedMultiplier)
    {
        // Arrange
        var provider = new MultiplierProvider();

        // Act
        var multiplier = provider.GetTimeMultiplier(isPeakHour);

        // Assert
        multiplier.Should().Be(expectedMultiplier);
    }

    [Theory]
    [InlineData(OccupancyLevel.Low, 1.0)]
    [InlineData(OccupancyLevel.Mid, 1.1)]
    [InlineData(OccupancyLevel.High, 1.25)]
    public void GetOccupancyMultiplier_ShouldReturnExpectedMultiplier(
        OccupancyLevel occupancyLevel,
        decimal expectedMultiplier)
    {
        // Arrange
        var provider = new MultiplierProvider();

        // Act
        var multiplier = provider.GetOccupancyMultiplier(occupancyLevel);

        // Assert
        multiplier.Should().Be(expectedMultiplier);
    }
}
