using FluentAssertions;
using FitnessReservation.Reservations.Services;
using Xunit;

namespace FitnessReservation.Reservations.Tests;

public sealed class PeakHourPolicyTests
{
    private readonly IPeakHourPolicy _sut = new PeakHourPolicy();

    [Theory]
    // before peak
    [InlineData("2030-01-01T17:59:00Z", false)]
    [InlineData("2030-01-01T17:00:00Z", false)]

    // start boundary
    [InlineData("2030-01-01T18:00:00Z", true)]
    [InlineData("2030-01-01T21:59:00Z", true)]

    // end boundary
    [InlineData("2030-01-01T22:00:00Z", false)]
    [InlineData("2030-01-01T23:00:00Z", false)]
    public void IsPeak_ShouldFollowUtcHourWindow(string utc, bool expected)
    {
        var dt = DateTime.Parse(utc, null, System.Globalization.DateTimeStyles.AdjustToUniversal | System.Globalization.DateTimeStyles.AssumeUniversal);

        _sut.IsPeak(dt).Should().Be(expected);
    }
}
