using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace FitnessReservation.Api.Tests;

public sealed class SessionsApiTests : IAsyncLifetime
{
    private CustomWebApplicationFactory _factory = default!;
    private HttpClient _client = default!;

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
    public async Task Post_Sessions_ShouldReturn201_AndSessionId()
    {
        var req = new
        {
            sport = "Yoga",
            startsAtUtc = DateTime.UtcNow.AddHours(2),
            capacity = 20
        };

        var res = await _client.PostAsJsonAsync("/sessions", req);

        res.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await res.Content.ReadFromJsonAsync<CreateSessionResponse>();
        body.Should().NotBeNull();
        body!.SessionId.Should().NotBe(Guid.Empty);
    }

    private sealed record CreateSessionResponse(Guid SessionId);
}
