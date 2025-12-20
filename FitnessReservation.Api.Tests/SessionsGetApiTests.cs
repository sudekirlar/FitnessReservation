using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace FitnessReservation.Api.Tests;

public sealed class SessionsGetApiTests : IAsyncLifetime
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
    public async Task Get_Sessions_WithoutLogin_ShouldReturn401()
    {
        var from = DateTime.UtcNow.AddHours(-3).ToString("O");
        var to = DateTime.UtcNow.AddHours(3).ToString("O");

        var url = $"/sessions?sport=Yoga&from={Uri.EscapeDataString(from)}&to={Uri.EscapeDataString(to)}";
        var res = await _client.GetAsync(url);

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Get_Sessions_WithLogin_ShouldReturn200_AndItemsWithPrice()
    {
        await RegisterAndLoginStandard();

        var from = DateTime.UtcNow.AddHours(-3).ToString("O");
        var to = DateTime.UtcNow.AddHours(3).ToString("O");

        var url = $"/sessions?sport=Yoga&from={Uri.EscapeDataString(from)}&to={Uri.EscapeDataString(to)}";

        var res = await _client.GetAsync(url);

        res.StatusCode.Should().Be(HttpStatusCode.OK, await res.Content.ReadAsStringAsync());

        var body = await res.Content.ReadFromJsonAsync<List<SessionItem>>();
        body.Should().NotBeNull();
        body!.Count.Should().BeGreaterThan(0);

        body.Should().OnlyContain(x => x.sport == "Yoga");
        body.Should().OnlyContain(x => x.capacity > 0);
        body.Should().OnlyContain(x => x.reservedCount >= 0);
        body.Should().OnlyContain(x => x.price.finalPrice > 0);
    }

    [Fact]
    public async Task Get_Sessions_WithInvalidDateRange_ShouldReturn400()
    {
        await RegisterAndLoginStandard();

        var from = DateTime.UtcNow.ToString("O");
        var to = DateTime.UtcNow.AddHours(-1).ToString("O");

        var url = $"/sessions?sport=Yoga&from={Uri.EscapeDataString(from)}&to={Uri.EscapeDataString(to)}";
        var res = await _client.GetAsync(url);

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private async Task RegisterAndLoginStandard()
    {
        var username = $"u_{Guid.NewGuid():N}";
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

    private sealed class SessionItem
    {
        public string sessionId { get; set; } = default!;
        public string sport { get; set; } = default!;
        public DateTime startsAtUtc { get; set; }
        public string instructorName { get; set; } = default!;
        public int capacity { get; set; }
        public int reservedCount { get; set; }
        public bool isPeak { get; set; }
        public string occupancyLevel { get; set; } = default!;
        public Price price { get; set; } = default!;
    }

    private sealed class Price
    {
        public decimal finalPrice { get; set; }
    }
}
