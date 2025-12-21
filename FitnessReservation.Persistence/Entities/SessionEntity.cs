using FitnessReservation.Pricing.Models;

namespace FitnessReservation.Persistence.Entities;

public sealed class SessionEntity
{
    public Guid SessionId { get; set; }
    public SportType Sport { get; set; }
    public DateTime StartsAtUtc { get; set; }
    public int Capacity { get; set; }
    public string InstructorName { get; set; } = default!;
}
