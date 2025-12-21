using System;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FitnessReservation.Persistence.Tests;

public sealed class DbContextSmokeTests
{
    [Fact]
    public void DbContext_can_create_schema_in_memory()
    {
        var conn = new SqliteConnection("DataSource=:memory:");
        conn.Open();

        var options = new DbContextOptionsBuilder<FitnessReservationDbContext>()
            .UseSqlite(conn)
            .Options;

        using var db = new FitnessReservationDbContext(options);

        var act = () => db.Database.EnsureCreated();

        act.Should().NotThrow();
    }
}
