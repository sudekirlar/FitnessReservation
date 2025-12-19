using System;
using FluentAssertions;
using FitnessReservation.Reservations.Repos;
using FitnessReservation.Reservations.Services;
using Xunit;

namespace FitnessReservation.Reservations.Tests;

public sealed class ReservationsServiceGuardTests
{
    [Fact]
    public void Ctor_NullSessions_ShouldThrow()
    {
        var act = () => new ReservationsService(
            null!,
            new InMemoryReservationRepository(),
            PricingTestFactory.Engine(),
            new FakeClock(DateTime.UtcNow));

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("sessions");
    }

    [Fact]
    public void Ctor_NullReservations_ShouldThrow()
    {
        var act = () => new ReservationsService(
            new InMemorySessionRepository(),
            null!,
            PricingTestFactory.Engine(),
            new FakeClock(DateTime.UtcNow));

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("reservations");
    }

    [Fact]
    public void Reserve_NullRequest_ShouldThrow()
    {
        var sut = new ReservationsService(
            new InMemorySessionRepository(),
            new InMemoryReservationRepository(),
            PricingTestFactory.Engine(),
            new FakeClock(DateTime.UtcNow));

        var act = () => sut.Reserve(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("request");
    }
}
