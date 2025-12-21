namespace FitnessReservation.Persistence.Entities;

public sealed class ReservationEntity
{
    public Guid ReservationId { get; set; }

    public Guid MemberId { get; set; }
    public Guid SessionId { get; set; }

    public decimal FinalPrice { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
