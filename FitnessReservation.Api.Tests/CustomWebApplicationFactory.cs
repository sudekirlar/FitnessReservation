using FitnessReservation.Persistence;
using FitnessReservation.Persistence.Entities;
using FitnessReservation.Pricing.Models;
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

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<FitnessReservationDbContext>();
            db.Database.EnsureCreated();

            SeedTestData(db);
        });
    }

    private static void SeedTestData(FitnessReservationDbContext db)
    {
        // MembershipCodes (Auth register tests student/premium için ileride lazım olabilir)
        UpsertMembershipCode(db, "STU-2025", MembershipType.Student, isActive: true);
        UpsertMembershipCode(db, "PRM-2025", MembershipType.Premium, isActive: true);
        UpsertMembershipCode(db, "STD-OPEN", MembershipType.Standard, isActive: true);

        // Sessions (API tests bunları bekliyor)
        UpsertSession(
            db,
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            SportType.Yoga,
            DateTime.UtcNow.AddHours(2),
            capacity: 100,
            instructorName: "Elif Hoca");

        UpsertSession(
            db,
            Guid.Parse("33333333-3333-3333-3333-333333333333"),
            SportType.Yoga,
            DateTime.UtcNow.AddHours(2),
            capacity: 1,
            instructorName: "Hasan Hoca");

        UpsertSession(
            db,
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            SportType.Yoga,
            DateTime.UtcNow.AddHours(-2),
            capacity: 10,
            instructorName: "Sibel Hoca");

        db.SaveChanges();
    }

    private static void UpsertMembershipCode(FitnessReservationDbContext db, string code, MembershipType type, bool isActive)
    {
        var existing = db.MembershipCodes.SingleOrDefault(x => x.Code == code);
        if (existing is null)
        {
            db.MembershipCodes.Add(new MembershipCodeEntity
            {
                Code = code,
                MembershipType = type,
                IsActive = isActive,
                UsedByMemberId = null
            });
        }
        else
        {
            existing.MembershipType = type;
            existing.IsActive = isActive;
        }
    }

    private static void UpsertSession(
        FitnessReservationDbContext db,
        Guid sessionId,
        SportType sport,
        DateTime startsAtUtc,
        int capacity,
        string instructorName)
    {
        var existing = db.Sessions.SingleOrDefault(x => x.SessionId == sessionId);
        if (existing is null)
        {
            db.Sessions.Add(new SessionEntity
            {
                SessionId = sessionId,
                Sport = sport,
                StartsAtUtc = startsAtUtc,
                Capacity = capacity,
                InstructorName = instructorName
            });
        }
        else
        {
            existing.Sport = sport;
            existing.StartsAtUtc = startsAtUtc;
            existing.Capacity = capacity;
            existing.InstructorName = instructorName;
        }
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
