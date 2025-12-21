using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace FitnessReservation.Api.Tests;

public sealed class ApiSmokeTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ApiSmokeTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_Health_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/health");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Post_PricingCalculate_ShouldReturnOk_AndPrice()
    {
        var request = new
        {
            sport = "Yoga",
            membership = "Student",
            isPeak = true,
            occupancy = "High"
        };

        var response = await _client.PostAsJsonAsync("/pricing/calculate", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());

        var body = await response.Content.ReadFromJsonAsync<PricingResponse>();
        body.Should().NotBeNull();
        body!.finalPrice.Should().BeGreaterThan(0);
    }

    private sealed class PricingResponse
    {
        public decimal finalPrice { get; set; }
    }
}
