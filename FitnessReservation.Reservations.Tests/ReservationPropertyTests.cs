using System;
using System.Linq;
using FluentAssertions;
using FitnessReservation.Reservations.Models;
using FitnessReservation.Reservations.Repos;
using FitnessReservation.Reservations.Services;
using FsCheck;
using FsCheck.Xunit;
using FsCheck.Fluent;

namespace FitnessReservation.Reservations.Tests;

public sealed class ReservationPropertyTests
{
    private static readonly DateTime Now =
        new DateTime(2030, 1, 1, 10, 0, 0, DateTimeKind.Utc);

    // Capacity küçük olsun ki test hızlı ve deterministik kalsın
    public static Arbitrary<int> Capacity()
        => Arb.From(Gen.Choose(1, 5));

    [Property(Arbitrary = new[] { typeof(ReservationPropertyTests) }, MaxTest = 100)]
    public void Active_reservations_should_never_exceed_capacity(int capacity)
    {
        // Arrange
        var sessionId = Guid.NewGuid();

        var sessions = new InMemorySessionRepository();
        sessions.Upsert(new ClassSession
        {
            SessionId = sessionId,
            Sport = FitnessReservation.Pricing.Models.SportType.Yoga,
            StartsAtUtc = Now.AddHours(2), // always future
            Capacity = capacity
        });

        var reservations = new InMemoryReservationRepository();
        var sut = BuildSut(sessions, reservations);

        // Act
        // capacity'den fazla sayıda farklı member deniyoruz (duplicate olmasın diye)
        var attempts = capacity + 10;
        var results = Enumerable.Range(0, attempts)
            .Select(i => sut.Reserve(new ReserveRequest
            {
                MemberId = $"m{i}",
                SessionId = sessionId,
                Membership = FitnessReservation.Pricing.Models.MembershipType.Standard
            }))
            .ToList();

        var successCount = results.Count(r => r.Success);

        // Assert
        successCount.Should().BeLessThanOrEqualTo(capacity);
        results.Skip(capacity).Any(r => r.Error == ReserveError.CapacityFull).Should().BeTrue();
    }

    private static ReservationsService BuildSut(
        InMemorySessionRepository sessions,
        InMemoryReservationRepository reservations)
    {
        var clock = new FakeClock(Now);
        var pricing = PricingTestFactory.Engine();
        return new ReservationsService(sessions, reservations, pricing, clock);
    }
}
