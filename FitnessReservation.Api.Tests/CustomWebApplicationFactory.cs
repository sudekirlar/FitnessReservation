using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace FitnessReservation.Api.Tests;

public sealed class CustomWebApplicationFactory
    : WebApplicationFactory<FitnessReservation.Api.Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
    }
}
