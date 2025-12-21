using FitnessReservation.Pricing.Models;

namespace FitnessReservation.Persistence.Entities;

public sealed class MemberEntity
{
    public Guid MemberId { get; set; }
    public string Username { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public MembershipType MembershipType { get; set; }
}
