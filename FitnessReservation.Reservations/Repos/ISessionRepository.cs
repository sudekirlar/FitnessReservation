namespace FitnessReservation.Reservations.Repos;

public interface ISessionRepository
{
    object? Get(Guid sessionId);
}
