using FitnessReservation.Pricing.Models;
using FitnessReservation.Reservations.Models;

namespace FitnessReservation.Reservations.Repos;

public interface ISessionRepository
{
    ClassSession? Get(Guid sessionId);

    IEnumerable<ClassSession> Query(
        SportType sport,
        DateTime fromUtc,
        DateTime toUtc);
}
