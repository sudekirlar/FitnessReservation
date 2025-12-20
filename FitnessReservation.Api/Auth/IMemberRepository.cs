namespace FitnessReservation.Api.Auth;

public interface IMemberRepository
{
    Member? FindByUsername(string username);
    Member? Get(Guid memberId);
    void Add(Member member);
}
