using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace FitnessReservation.Api.Tests;

public sealed class AuthApiTests : IAsyncLifetime
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
    public async Task Register_Login_Me_Logout_Flow_ShouldWork()
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

        var login = await _client.PostAsJsonAsync("/auth/login", new
        {
            username,
            password
        });

        login.StatusCode.Should().Be(HttpStatusCode.OK, await login.Content.ReadAsStringAsync());

        var me = await _client.GetAsync("/me");
        me.StatusCode.Should().Be(HttpStatusCode.OK, await me.Content.ReadAsStringAsync());

        var logout = await _client.PostAsync("/auth/logout", content: null);
        logout.StatusCode.Should().Be(HttpStatusCode.OK);

        var me2 = await _client.GetAsync("/me");
        me2.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
