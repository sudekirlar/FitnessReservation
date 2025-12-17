using System;
using FluentAssertions;
using FitnessReservation.Pricing.Models;
using FitnessReservation.Pricing.Services;
using Xunit;

namespace FitnessReservation.Pricing.Tests;

public sealed class MultiplierProviderNegativeTests
{
    [Fact]
    public void GetMembershipMultiplier_UnknownMembership_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var provider = new MultiplierProvider();
        var unknownMembership = (MembershipType)(-1);

        // Act
        var act = () => provider.GetMembershipMultiplier(unknownMembership);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("membershipType");
    }

    [Fact]
    public void GetOccupancyMultiplier_UnknownOccupancy_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var provider = new MultiplierProvider();
        var unknownOccupancy = (OccupancyLevel)42;

        // Act
        var act = () => provider.GetOccupancyMultiplier(unknownOccupancy);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("occupancyLevel");
    }
}
