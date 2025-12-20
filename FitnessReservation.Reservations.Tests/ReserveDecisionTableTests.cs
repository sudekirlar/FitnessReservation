using System;
using System.Collections.Generic;
using FluentAssertions;
using FitnessReservation.Reservations.Models;
using FitnessReservation.Reservations.Repos;
using FitnessReservation.Reservations.Services;
using Xunit;

namespace FitnessReservation.Reservations.Tests;

public sealed class ReserveDecisionTableTests
{
    private static readonly DateTime Now =
        new DateTime(2030, 1, 1, 10, 0, 0, DateTimeKind.Utc);

    public static IEnumerable<object[]> Cases()
    {
        yield return new object[] { "Case1_SessionNotFound", false, true, false, true, ReserveError.SessionNotFound };
        yield return new object[] { "Case2_SessionInPast", true, false, false, true, ReserveError.SessionInPast };
        yield return new object[] { "Case3_DuplicateReservation", true, true, true, true, ReserveError.DuplicateReservation };
        yield return new object[] { "Case4_CapacityFull", true, true, false, false, ReserveError.CapacityFull };
        yield return new object[] { "Case5_Success", true, true, false, true, ReserveError.None };
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void Reserve_ShouldFollowDecisionTable(
        string caseId,
        bool exists,
        bool future,
        bool duplicate,
        bool capacityAvailable,
        ReserveError expected)
    {
        var sessionId = Guid.NewGuid();
        var memberId = "m1";

        var sessions = new InMemorySessionRepository();
        if (exists)
        {
            sessions.Upsert(new ClassSession
            {
                SessionId = sessionId,
                Sport = FitnessReservation.Pricing.Models.SportType.Yoga,
                StartsAtUtc = future ? Now.AddHours(2) : Now.AddHours(-1),
                Capacity = 1,
                InstructorName = "Elif Hoca"
            });
        }

        var reservations = new InMemoryReservationRepository();

        if (exists && future && !duplicate && !capacityAvailable)
        {
            var fill = BuildSut(sessions, reservations);
            var r = fill.Reserve(new ReserveRequest
            {
                MemberId = "someone-else",
                SessionId = sessionId,
                Membership = FitnessReservation.Pricing.Models.MembershipType.Standard
            });
            r.Success.Should().BeTrue("precondition: fill capacity should succeed");
        }

        if (exists && future && duplicate)
        {
            var first = BuildSut(sessions, reservations).Reserve(new ReserveRequest
            {
                MemberId = memberId,
                SessionId = sessionId,
                Membership = FitnessReservation.Pricing.Models.MembershipType.Standard
            });
            first.Success.Should().BeTrue("precondition: first reservation should succeed");
        }

        var sut = BuildSut(sessions, reservations);

        var result = sut.Reserve(new ReserveRequest
        {
            MemberId = memberId,
            SessionId = sessionId,
            Membership = FitnessReservation.Pricing.Models.MembershipType.Standard
        });

        result.Error.Should().Be(expected, because: caseId);

        if (expected == ReserveError.None)
        {
            result.Success.Should().BeTrue(because: caseId);
            result.ReservationId.Should().NotBeNull(because: caseId);
            result.PriceSnapshot.Should().NotBeNull(because: caseId);
        }
        else
        {
            result.Success.Should().BeFalse(because: caseId);
        }
    }

    private static ReservationsService BuildSut(
        InMemorySessionRepository sessions,
        InMemoryReservationRepository reservations)
    {
        return new ReservationsService(
            sessions,
            reservations,
            PricingTestFactory.Engine(),
            new FakeClock(Now),
            new PeakHourPolicy(),
            new OccupancyClassifier());
    }
}
