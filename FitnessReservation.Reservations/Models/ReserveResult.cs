namespace FitnessReservation.Reservations.Models;

public sealed class ReserveResult
{
    public bool Success { get; init; }
    public ReserveError Error { get; init; }

    public static ReserveResult Fail(ReserveError error) => new()
    {
        Success = false,
        Error = error
    };
}
