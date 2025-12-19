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
}