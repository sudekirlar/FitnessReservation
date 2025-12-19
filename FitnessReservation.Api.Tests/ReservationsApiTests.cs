using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace FitnessReservation.Api.Tests;

public sealed class ReservationsApiTests : IAsyncLifetime
{
    private CustomWebApplicationFactory _factory = default!;
    private HttpClient _client = default!;

    private static readonly Guid FutureGeneralSessionId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static readonly Guid FutureCapacity1SessionId =
        Guid.Parse("33333333-3333-3333-3333-333333333333");

    private static readonly Guid PastSessionId =
        Guid.Parse("22222222-2222-2222-2222-222222222222");

    public Task InitializeAsync()
    {
        // Her teste yeni instance.
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task Post_Reservations_Success_ShouldReturn201()
    {
        var req = new
        {
            memberId = "m1",
            sessionId = FutureGeneralSessionId,
            membership = "Standard"
        };

        var res = await _client.PostAsJsonAsync("/reservations", req);

        res.StatusCode.Should().Be(HttpStatusCode.Created, await res.Content.ReadAsStringAsync());
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
            sessionId = FutureGeneralSessionId,
            membership = "Standard"
        };

        var first = await _client.PostAsJsonAsync("/reservations", req);
        first.StatusCode.Should().Be(HttpStatusCode.Created, await first.Content.ReadAsStringAsync());

        var second = await _client.PostAsJsonAsync("/reservations", req);
        second.StatusCode.Should().Be(HttpStatusCode.Conflict, await second.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Post_Reservations_CapacityFull_ShouldReturn409()
    {
        // capacity=1 olan session'a iki farklı member
        var r1 = await _client.PostAsJsonAsync("/reservations", new
        {
            memberId = "m1",
            sessionId = FutureCapacity1SessionId,
            membership = "Standard"
        });
        r1.StatusCode.Should().Be(HttpStatusCode.Created, await r1.Content.ReadAsStringAsync());

        var r2 = await _client.PostAsJsonAsync("/reservations", new
        {
            memberId = "m2",
            sessionId = FutureCapacity1SessionId,
            membership = "Standard"
        });
        r2.StatusCode.Should().Be(HttpStatusCode.Conflict, await r2.Content.ReadAsStringAsync());
    }
}
