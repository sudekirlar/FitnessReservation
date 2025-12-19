using FitnessReservation.Reservations.Models;

namespace FitnessReservation.Reservations.Repos;

public interface ISessionRepository
{
    ClassSession? Get(Guid sessionId);
}
