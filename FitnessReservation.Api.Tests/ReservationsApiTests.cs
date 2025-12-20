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
    public async Task Post_Reservations_WithoutLogin_ShouldReturn401()
    {
        var res = await _client.PostAsJsonAsync("/reservations", new
        {
            sessionId = FutureGeneralSessionId
        });

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Post_Reservations_Success_ShouldReturn201()
    {
        await RegisterAndLoginStandard("u1");

        var res = await _client.PostAsJsonAsync("/reservations", new
        {
            sessionId = FutureGeneralSessionId
        });

        res.StatusCode.Should().Be(HttpStatusCode.Created, await res.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Post_Reservations_SessionNotFound_ShouldReturn404()
    {
        await RegisterAndLoginStandard("u1");

        var res = await _client.PostAsJsonAsync("/reservations", new
        {
            sessionId = Guid.NewGuid()
        });

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Post_Reservations_SessionInPast_ShouldReturn400()
    {
        await RegisterAndLoginStandard("u1");

        var res = await _client.PostAsJsonAsync("/reservations", new
        {
            sessionId = PastSessionId
        });

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_Reservations_Duplicate_ShouldReturn409()
    {
        await RegisterAndLoginStandard("u1");

        var first = await _client.PostAsJsonAsync("/reservations", new { sessionId = FutureGeneralSessionId });
        first.StatusCode.Should().Be(HttpStatusCode.Created, await first.Content.ReadAsStringAsync());

        var second = await _client.PostAsJsonAsync("/reservations", new { sessionId = FutureGeneralSessionId });
        second.StatusCode.Should().Be(HttpStatusCode.Conflict, await second.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Post_Reservations_CapacityFull_ShouldReturn409_ForSecondUser()
    {
        // user1 reserves
        await RegisterAndLoginStandard("u1");
        var r1 = await _client.PostAsJsonAsync("/reservations", new { sessionId = FutureCapacity1SessionId });
        r1.StatusCode.Should().Be(HttpStatusCode.Created, await r1.Content.ReadAsStringAsync());

        // user2 reserves same capacity=1 session => 409
        await LogoutIfAny();
        await RegisterAndLoginStandard("u2");

        var r2 = await _client.PostAsJsonAsync("/reservations", new { sessionId = FutureCapacity1SessionId });
        r2.StatusCode.Should().Be(HttpStatusCode.Conflict, await r2.Content.ReadAsStringAsync());
    }

    private async Task RegisterAndLoginStandard(string prefix)
    {
        var username = $"{prefix}_{Guid.NewGuid():N}";
        var password = "P@ssw0rd-1";

        var reg = await _client.PostAsJsonAsync("/auth/register", new
        {
            username,
            password,
            membershipType = "Standard",
            membershipCode = (string?)null
        });
        reg.StatusCode.Should().Be(HttpStatusCode.Created, await reg.Content.ReadAsStringAsync());

        var login = await _client.PostAsJsonAsync("/auth/login", new { username, password });
        login.StatusCode.Should().Be(HttpStatusCode.OK, await login.Content.ReadAsStringAsync());
    }

    private async Task LogoutIfAny()
    {
        _ = await _client.PostAsync("/auth/logout", content: null);
    }
}
