using System;
using System.Linq;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FitnessReservation.Persistence;
using FitnessReservation.Persistence.Repos;

namespace FitnessReservation.Persistence.Tests;

public sealed class ReservationPricePersistenceTests
{
    [Fact]
    public void EfReservationRepository_Add_should_persist_final_price_and_created_at()
    {
        var conn = new SqliteConnection("DataSource=:memory:");
        conn.Open();

        var options = new DbContextOptionsBuilder<FitnessReservationDbContext>()
            .UseSqlite(conn)
            .Options;

        using var db = new FitnessReservationDbContext(options);
        db.Database.EnsureCreated();

        var repo = new EfReservationRepository(db);

        var memberId = Guid.NewGuid().ToString();
        var sessionId = Guid.NewGuid();
        var price = 123.45m;
        var createdAt = new DateTime(2030, 1, 1, 10, 0, 0, DateTimeKind.Utc);

        repo.Add(memberId, sessionId, price, createdAt);

        var row = db.Reservations.Single(r => r.MemberId.ToString() == memberId && r.SessionId == sessionId);
        row.FinalPrice.Should().Be(price);
        row.CreatedAtUtc.Should().Be(createdAt);
    }
}
