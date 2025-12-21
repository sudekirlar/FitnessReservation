using FitnessReservation.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitnessReservation.Persistence;

public sealed class FitnessReservationDbContext : DbContext
{
    public FitnessReservationDbContext(DbContextOptions<FitnessReservationDbContext> options)
        : base(options) { }

    public DbSet<MemberEntity> Members => Set<MemberEntity>();
    public DbSet<MembershipCodeEntity> MembershipCodes => Set<MembershipCodeEntity>();
    public DbSet<SessionEntity> Sessions => Set<SessionEntity>();
    public DbSet<ReservationEntity> Reservations => Set<ReservationEntity>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // Members
        b.Entity<MemberEntity>(e =>
        {
            e.ToTable("Members");
            e.HasKey(x => x.MemberId);

            e.Property(x => x.MemberId)
                .HasConversion(
                    v => v.ToString(),
                    v => Guid.Parse(v))
                .ValueGeneratedNever();

            e.Property(x => x.Username)
                .IsRequired();

            e.HasIndex(x => x.Username)
                .IsUnique();

            e.Property(x => x.PasswordHash)
                .IsRequired();

            e.Property(x => x.MembershipType)
                .IsRequired();
        });

        // MembershipCodes
        b.Entity<MembershipCodeEntity>(e =>
        {
            e.ToTable("MembershipCodes");
            e.HasKey(x => x.Code);

            e.Property(x => x.Code)
                .IsRequired();

            e.Property(x => x.MembershipType)
                .IsRequired();

            e.Property(x => x.IsActive)
                .IsRequired();

            e.Property(x => x.UsedByMemberId)
                .HasConversion(
                    v => v.HasValue ? v.Value.ToString() : null,
                    v => string.IsNullOrWhiteSpace(v) ? null : Guid.Parse(v));
        });

        // Sessions
        b.Entity<SessionEntity>(e =>
        {
            e.ToTable("Sessions");
            e.HasKey(x => x.SessionId);

            e.Property(x => x.SessionId)
                .HasConversion(
                    v => v.ToString(),
                    v => Guid.Parse(v))
                .ValueGeneratedNever();

            e.Property(x => x.Sport)
                .IsRequired();

            e.Property(x => x.StartsAtUtc)
                .IsRequired();

            e.Property(x => x.Capacity)
                .IsRequired();

            e.Property(x => x.InstructorName)
                .IsRequired();
        });

        // Reservations
        b.Entity<ReservationEntity>(e =>
        {
            e.ToTable("Reservations");
            e.HasKey(x => x.ReservationId);

            e.Property(x => x.ReservationId)
                .HasConversion(
                    v => v.ToString(),
                    v => Guid.Parse(v))
                .ValueGeneratedNever();

            e.Property(x => x.MemberId)
                .HasConversion(
                    v => v.ToString(),
                    v => Guid.Parse(v))
                .IsRequired();

            e.Property(x => x.SessionId)
                .HasConversion(
                    v => v.ToString(),
                    v => Guid.Parse(v))
                .IsRequired();

            e.Property(x => x.FinalPrice)
                .HasColumnType("TEXT")
                .IsRequired();

            e.Property(x => x.CreatedAtUtc)
                .IsRequired();

            e.HasIndex(x => x.SessionId);

            e.HasIndex(x => new { x.MemberId, x.SessionId })
                .IsUnique();
        });
    }
}
