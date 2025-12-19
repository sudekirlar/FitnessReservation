namespace FitnessReservation.Reservations.Models;

public enum ReserveError
{
    None = 0,
    SessionNotFound = 1,
    SessionInPast = 2,
    DuplicateReservation = 3,
}
