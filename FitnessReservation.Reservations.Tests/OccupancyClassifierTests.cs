using FluentAssertions;
using FitnessReservation.Pricing.Models;
using FitnessReservation.Reservations.Services;
using Xunit;

namespace FitnessReservation.Reservations.Tests;

public sealed class OccupancyClassifierTests
{
    private readonly IOccupancyClassifier _sut = new OccupancyClassifier();

    [Theory]
    // <0.50 => Low
    [InlineData(0, 10, OccupancyLevel.Low)]
    [InlineData(4, 10, OccupancyLevel.Low)]

    // 0.50 <= ratio < 0.80 => Mid
    [InlineData(5, 10, OccupancyLevel.Mid)]
    [InlineData(7, 10, OccupancyLevel.Mid)]

    // >=0.80 => High
    [InlineData(8, 10, OccupancyLevel.High)]
    [InlineData(10, 10, OccupancyLevel.High)]
    public void Classify_ShouldMapFillRatioToLevel(int reserved, int capacity, OccupancyLevel expected)
    {
        _sut.Classify(reserved, capacity).Should().Be(expected);
    }

    [Fact]
    public void Classify_CapacityZero_ShouldThrow()
    {
        var act = () => _sut.Classify(reservedCount: 0, capacity: 0);
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("capacity");
    }

    [Fact]
    public void Classify_NegativeReserved_ShouldThrow()
    {
        var act = () => _sut.Classify(reservedCount: -1, capacity: 10);
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("reservedCount");
    }
}
