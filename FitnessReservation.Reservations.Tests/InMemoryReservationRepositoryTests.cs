using System;
using FluentAssertions;
using FitnessReservation.Reservations.Repos;
using Xunit;

namespace FitnessReservation.Reservations.Tests;

public sealed class InMemoryReservationRepositoryTests
{
    [Fact]
    public void Add_WhenSameMemberAndSessionAddedTwice_ShouldThrow()
    {
        var repo = new InMemoryReservationRepository();
        var sessionId = Guid.NewGuid();

        repo.Add("m1", sessionId);

        var act = () => repo.Add("m1", sessionId);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Duplicate reservation*");
    }
}
