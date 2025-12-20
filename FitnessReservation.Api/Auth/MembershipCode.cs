using FitnessReservation.Pricing.Models;

namespace FitnessReservation.Api.Auth;

public sealed class MembershipCode
{
    public string Code { get; init; } = default!;
    public MembershipType MembershipType { get; init; }
    public bool IsActive { get; init; } = true;

    public Guid? UsedByMemberId { get; set; }
}
