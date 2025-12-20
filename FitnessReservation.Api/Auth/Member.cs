using FitnessReservation.Pricing.Models;

namespace FitnessReservation.Api.Auth;

public sealed class Member
{
    public Guid MemberId { get; init; }
    public string Username { get; init; } = default!;
    public string PasswordHash { get; init; } = default!;
    public MembershipType MembershipType { get; init; }
}
