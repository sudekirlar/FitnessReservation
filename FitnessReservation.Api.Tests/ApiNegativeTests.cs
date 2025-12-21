using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace FitnessReservation.Api.Tests;

public sealed class ApiNegativeTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ApiNegativeTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Post_PricingCalculate_WithInvalidEnum_ShouldReturnBadRequest()
    {
        var request = new
        {
            sport = "Yoga",
            membership = "Gold", // invalid
            isPeak = true,
            occupancy = "High"
        };

        var response = await _client.PostAsJsonAsync("/pricing/calculate", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
