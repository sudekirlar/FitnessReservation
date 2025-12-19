using FitnessReservation.Pricing.Models;

namespace FitnessReservation.Reservations.Models;

public sealed class ReserveRequest
{
    public string MemberId { get; init; } = default!;
    public Guid SessionId { get; init; }
    public MembershipType Membership { get; init; }
}
