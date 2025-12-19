namespace FitnessReservation.Reservations.Services;

public interface IClock
{
    DateTime UtcNow { get; }
}
