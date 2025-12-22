using System;
using FluentAssertions;
using FitnessReservation.Pricing.Models;
using FitnessReservation.Pricing.Services;
using FitnessReservation.Reservations.Models;
using FitnessReservation.Reservations.Repos;
using FitnessReservation.Reservations.Services;
using Moq;
using Xunit;

namespace FitnessReservation.Reservations.Tests;

public sealed class ReservationsServiceMockingTests
{
    [Fact]
    public void Reserve_WhenSuccessful_ShouldPersistExactlyOnce_WithExpectedArguments()
    {
        // Arrange
        var now = new DateTime(2030, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var sessionId = Guid.NewGuid();
        var memberId = "m1";

        // Use a real in-memory session repository (so business setup is realistic)
        var sessions = new InMemorySessionRepository();
        sessions.Upsert(new ClassSession
        {
            SessionId = sessionId,
            Sport = SportType.Yoga,
            StartsAtUtc = now.AddHours(2), // future
            Capacity = 10,
            InstructorName = "Elif Hoca"
        });

        // Mock IReservationRepository to verify interactions (this is the "mocking" requirement)
        var reservations = new Mock<IReservationRepository>(MockBehavior.Strict);

        // Setup expected calls used by ReservationsService
        reservations
            .Setup(r => r.Exists(memberId, sessionId))
            .Returns(false);

        reservations
            .Setup(r => r.CountBySession(sessionId))
            .Returns(0);

        // Capture arguments of Add(...) for stronger assertions
        string? capturedMemberId = null;
        Guid capturedSessionId = default;
        decimal capturedFinalPrice = default;
        DateTime capturedCreatedAtUtc = default;

        reservations
            .Setup(r => r.Add(
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<decimal>(),
                It.IsAny<DateTime>()))
            .Callback<string, Guid, decimal, DateTime>((mid, sid, price, createdAt) =>
            {
                capturedMemberId = mid;
                capturedSessionId = sid;
                capturedFinalPrice = price;
                capturedCreatedAtUtc = createdAt;
            });

        // Mock clock too: ensures createdAtUtc comes from dependency (not DateTime.UtcNow hidden)
        var clock = new Mock<IClock>(MockBehavior.Strict);
        clock.SetupGet(c => c.UtcNow).Returns(now);

        var pricing = new PricingEngine(new BasePriceProvider(), new MultiplierProvider());

        var sut = new ReservationsService(
            sessions,
            reservations.Object,
            pricing,
            clock.Object,
            new PeakHourPolicy(),
            new OccupancyClassifier());

        var req = new ReserveRequest
        {
            MemberId = memberId,
            SessionId = sessionId,
            Membership = MembershipType.Standard
        };

        // Act
        var result = sut.Reserve(req);

        // Assert (state-based)
        result.Success.Should().BeTrue();
        result.Error.Should().Be(ReserveError.None);
        result.PriceSnapshot.Should().NotBeNull();
        result.PriceSnapshot!.FinalPrice.Should().BeGreaterThan(0);

        // Assert (interaction-based) — what mocking frameworks are good for
        reservations.Verify(r => r.Exists(memberId, sessionId), Times.Once);
        reservations.Verify(r => r.CountBySession(sessionId), Times.Once);

        // Add must be called exactly once with consistent arguments
        reservations.Verify(r => r.Add(
            It.IsAny<string>(),
            It.IsAny<Guid>(),
            It.IsAny<decimal>(),
            It.IsAny<DateTime>()),
            Times.Once);

        capturedMemberId.Should().Be(memberId);
        capturedSessionId.Should().Be(sessionId);
        capturedFinalPrice.Should().Be(result.PriceSnapshot!.FinalPrice);
        capturedFinalPrice.Should().BeGreaterThan(0);
        capturedCreatedAtUtc.Should().Be(now);

        // Clock must be read at least once (usually exactly once, but allow >=1 if implementation evolves)
        clock.VerifyGet(c => c.UtcNow, Times.AtLeastOnce);

        // Ensure no unexpected calls happened (Strict mocks + VerifyNoOtherCalls)
        reservations.VerifyNoOtherCalls();
        clock.VerifyNoOtherCalls();
    }
}
