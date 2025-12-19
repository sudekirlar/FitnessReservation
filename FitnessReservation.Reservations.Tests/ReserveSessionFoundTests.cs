using FluentAssertions;
using FitnessReservation.Reservations.Models;
using FitnessReservation.Reservations.Repos;
using FitnessReservation.Reservations.Services;
using Xunit;

namespace FitnessReservation.Reservations.Tests;

public sealed class ReserveSessionFoundTests
{
    [Fact]
    public void Reserve_WhenSessionExistsAndIsInFuture_ShouldSucceed()
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
        var result = sut.Reserve(request);

        // Assert
        result.Success.Should().BeTrue();
        result.Error.Should().Be(ReserveError.None);
        result.ReservationId.Should().NotBeNull();
        result.PriceSnapshot.Should().NotBeNull();
        result.PriceSnapshot!.FinalPrice.Should().BeGreaterThan(0);
    }


}