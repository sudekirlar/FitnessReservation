namespace FitnessReservation.Api.Auth;

public interface IMembershipCodeRepository
{
    MembershipCode? Get(string code);
    void MarkUsed(string code, Guid memberId);
}
