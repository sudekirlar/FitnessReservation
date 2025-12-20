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
        var now = new DateTime(2030, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var sessionId = Guid.NewGuid();

        var sessions = new InMemorySessionRepository();
        sessions.Upsert(new ClassSession
        {
            SessionId = sessionId,
            Sport = FitnessReservation.Pricing.Models.SportType.Yoga,
            StartsAtUtc = now.AddHours(2),
            Capacity = 10,
            InstructorName = "Elif Hoca"
        });

        var reservations = new InMemoryReservationRepository();
        var sut = new ReservationsService(
            sessions,
            reservations,
            PricingTestFactory.Engine(),
            new FakeClock(now),
            new PeakHourPolicy(),
            new OccupancyClassifier());

        var result = sut.Reserve(new ReserveRequest
        {
            MemberId = "m1",
            SessionId = sessionId,
            Membership = FitnessReservation.Pricing.Models.MembershipType.Standard
        });

        result.Success.Should().BeTrue();
        result.Error.Should().Be(ReserveError.None);
        result.ReservationId.Should().NotBeNull();
        result.PriceSnapshot.Should().NotBeNull();
        result.PriceSnapshot!.FinalPrice.Should().BeGreaterThan(0);
    }
}
