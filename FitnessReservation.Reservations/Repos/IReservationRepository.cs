namespace FitnessReservation.Reservations.Repos;

public interface IReservationRepository
{
    bool Exists(string memberId, Guid sessionId);
    int CountBySession(Guid sessionId);

    void Add(string memberId, Guid sessionId, decimal finalPrice, DateTime createdAtUtc);
}
