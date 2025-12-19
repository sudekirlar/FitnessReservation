using FluentAssertions;
using FitnessReservation.Reservations.Models;
using FitnessReservation.Reservations.Repos;
using FitnessReservation.Reservations.Services;
using Xunit;

namespace FitnessReservation.Reservations.Tests;

public sealed class ReserveSessionNotFoundTests
{
    [Fact]
    public void Reserve_WhenSessionDoesNotExist_ShouldReturnSessionNotFound()
    {
        // Arrange
        var sessions = new InMemorySessionRepository(); // empty
        var reservations = new InMemoryReservationRepository();
        var clock = new FakeClock(new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var pricing = PricingTestFactory.Engine();

        var sut = new ReservationsService(sessions, reservations, pricing, clock);

        var request = new ReserveRequest
        {
            MemberId = "m1",
            SessionId = Guid.NewGuid(),
            Membership = FitnessReservation.Pricing.Models.MembershipType.Standard
        };

        // Act
        var result = sut.Reserve(request);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Be(ReserveError.SessionNotFound);
    }

    [Fact]
    public void Reserve_WhenSessionIsInPast_ShouldReturnSessionInPast()
    {
        // Arrange
        var now = new DateTime(2030, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var sessionId = Guid.NewGuid();

        var sessions = new InMemorySessionRepository();
        sessions.Upsert(new ClassSession
        {
            SessionId = sessionId,
            Sport = FitnessReservation.Pricing.Models.SportType.Yoga,
            StartsAtUtc = now.AddHours(-1), 
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
        var result = sut.Reserve(request);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Be(ReserveError.SessionInPast);
    }

}