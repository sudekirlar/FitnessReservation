using FitnessReservation.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FitnessReservation.Api.Tests;

public sealed class CustomWebApplicationFactory
    : WebApplicationFactory<FitnessReservation.Api.Program>
{
    private SqliteConnection? _conn;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // IMPORTANT: Program.cs'deki migrate/seed bloklarını bypass etmek için
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<FitnessReservationDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            _conn = new SqliteConnection("DataSource=:memory:");
            _conn.Open();

            services.AddDbContext<FitnessReservationDbContext>(opt => opt.UseSqlite(_conn));

            // Create schema (in-memory)
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<FitnessReservationDbContext>();
            db.Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _conn?.Dispose();
            _conn = null;
        }
    }
}
