using FitnessReservation.Pricing.Models;

namespace FitnessReservation.Persistence.Entities;

public sealed class MembershipCodeEntity
{
    public string Code { get; set; } = default!;
    public MembershipType MembershipType { get; set; }
    public bool IsActive { get; set; } = true;

    public Guid? UsedByMemberId { get; set; }
}
