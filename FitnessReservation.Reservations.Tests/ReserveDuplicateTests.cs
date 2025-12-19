using FluentAssertions;
using FitnessReservation.Reservations.Models;
using FitnessReservation.Reservations.Repos;
using FitnessReservation.Reservations.Services;
using Xunit;

namespace FitnessReservation.Reservations.Tests;

public sealed class ReserveDuplicateTests
{
    [Fact]
    public void Reserve_WhenSameMemberReservesSameSessionTwice_ShouldReturnDuplicateReservation()
    {
        // Arrange
        var now = new DateTime(2030, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var sessionId = Guid.NewGuid();

        var sessions = new InMemorySessionRepository();
        sessions.Upsert(new ClassSession
        {
            SessionId = sessionId,
            Sport = FitnessReservation.Pricing.Models.SportType.Yoga,
            StartsAtUtc = now.AddHours(2),
            Capacity = 10
        });

        var reservations = new InMemoryReservationRepository();
        var clock = new FakeClock(now);
        var pricing = PricingTestFactory.Engine();

        var sut = new ReservationsService(sessions, reservations, pricing, clock);

        var request = new ReserveRequest
        {
            MemberId = "m1",
            SessionId = sessionId,
            Membership = FitnessReservation.Pricing.Models.MembershipType.Standard
        };

        // Act
        var first = sut.Reserve(request);
        var second = sut.Reserve(request);

        // Assert
        first.Success.Should().BeTrue();

        second.Success.Should().BeFalse();
        second.Error.Should().Be(ReserveError.DuplicateReservation);
    }
}
