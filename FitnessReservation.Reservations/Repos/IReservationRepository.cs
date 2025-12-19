namespace FitnessReservation.Reservations.Repos;

public interface IReservationRepository
{
    bool Exists(string memberId, Guid sessionId);
    void Add(string memberId, Guid sessionId);
}
