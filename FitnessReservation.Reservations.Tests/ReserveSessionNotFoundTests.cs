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
        var sessions = new InMemorySessionRepository();
        var sut = new ReservationsService(
            sessions,
            new InMemoryReservationRepository(),
            PricingTestFactory.Engine(),
            new FakeClock(new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
            new PeakHourPolicy(),
            new OccupancyClassifier());

        var result = sut.Reserve(new ReserveRequest
        {
            MemberId = "m1",
            SessionId = Guid.NewGuid(),
            Membership = FitnessReservation.Pricing.Models.MembershipType.Standard
        });

        result.Success.Should().BeFalse();
        result.Error.Should().Be(ReserveError.SessionNotFound);
    }

    [Fact]
    public void Reserve_WhenSessionStartsExactlyNow_ShouldReturnSessionInPast()
    {
        var now = new DateTime(2030, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var sessionId = Guid.NewGuid();

        var sessions = new InMemorySessionRepository();
        sessions.Upsert(new ClassSession
        {
            SessionId = sessionId,
            Sport = FitnessReservation.Pricing.Models.SportType.Yoga,
            StartsAtUtc = now,
            Capacity = 10,
            InstructorName = "Elif Hoca"
        });

        var sut = new ReservationsService(
            sessions,
            new InMemoryReservationRepository(),
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

        result.Success.Should().BeFalse();
        result.Error.Should().Be(ReserveError.SessionInPast);
    }
}
