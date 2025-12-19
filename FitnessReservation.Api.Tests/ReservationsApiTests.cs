using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace FitnessReservation.Api.Tests;

public sealed class ReservationsApiTests
    : IClassFixture<WebApplicationFactory<FitnessReservation.Api.Program>>
{
    private readonly HttpClient _client;

    private static readonly Guid FutureSessionId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static readonly Guid PastSessionId =
        Guid.Parse("22222222-2222-2222-2222-222222222222");

    public ReservationsApiTests(WebApplicationFactory<FitnessReservation.Api.Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Post_Reservations_Success_ShouldReturn201()
    {
        var req = new
        {
            memberId = "m1",
            sessionId = FutureSessionId,
            membership = "Standard"
        };

        var res = await _client.PostAsJsonAsync("/reservations", req);

        res.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Post_Reservations_SessionNotFound_ShouldReturn404()
    {
        var req = new
        {
            memberId = "m1",
            sessionId = Guid.NewGuid(),
            membership = "Standard"
        };

        var res = await _client.PostAsJsonAsync("/reservations", req);

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Post_Reservations_SessionInPast_ShouldReturn400()
    {
        var req = new
        {
            memberId = "m1",
            sessionId = PastSessionId,
            membership = "Standard"
        };

        var res = await _client.PostAsJsonAsync("/reservations", req);

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_Reservations_Duplicate_ShouldReturn409()
    {
        var req = new
        {
            memberId = "m1",
            sessionId = FutureSessionId,
            membership = "Standard"
        };

        var first = await _client.PostAsJsonAsync("/reservations", req);
        first.StatusCode.Should().Be(HttpStatusCode.Created);

        var second = await _client.PostAsJsonAsync("/reservations", req);
        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Post_Reservations_CapacityFull_ShouldReturn409()
    {
        var r1 = await _client.PostAsJsonAsync("/reservations", new
        {
            memberId = "m1",
            sessionId = FutureSessionId,
            membership = "Standard"
        });
        r1.StatusCode.Should().Be(HttpStatusCode.Created);

        var r2 = await _client.PostAsJsonAsync("/reservations", new
        {
            memberId = "m2",
            sessionId = FutureSessionId,
            membership = "Standard"
        });
        r2.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
