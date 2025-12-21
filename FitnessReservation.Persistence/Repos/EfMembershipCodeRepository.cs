using FitnessReservation.Api.Auth;
using FitnessReservation.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitnessReservation.Persistence;

public sealed class EfMembershipCodeRepository : IMembershipCodeRepository
{
    private readonly FitnessReservationDbContext _db;

    public EfMembershipCodeRepository(FitnessReservationDbContext db)
        => _db = db;

    public MembershipCode? Get(string code)
    {
        var e = _db.MembershipCodes.AsNoTracking()
            .FirstOrDefault(x => x.Code.ToLower() == code.ToLower());

        return e is null ? null : new MembershipCode
        {
            Code = e.Code,
            MembershipType = e.MembershipType,
            IsActive = e.IsActive,
            UsedByMemberId = e.UsedByMemberId
        };
    }

    public void MarkUsed(string code, Guid memberId)
    {
        var e = _db.MembershipCodes
            .FirstOrDefault(x => x.Code.ToLower() == code.ToLower());

        if (e is null)
            throw new InvalidOperationException("CodeNotFound");

        e.UsedByMemberId = memberId;
        _db.SaveChanges();
    }
}
