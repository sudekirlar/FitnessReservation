using FitnessReservation.Api.Auth;
using FitnessReservation.Auth;
using FitnessReservation.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitnessReservation.Persistence;

public sealed class EfMemberRepository : IMemberRepository
{
    private readonly FitnessReservationDbContext _db;

    public EfMemberRepository(FitnessReservationDbContext db)
        => _db = db;

    public Member? FindByUsername(string username)
    {
        var e = _db.Members.AsNoTracking()
            .FirstOrDefault(x => x.Username.ToLower() == username.ToLower());

        return e is null ? null : new Member
        {
            MemberId = e.MemberId,
            Username = e.Username,
            PasswordHash = e.PasswordHash,
            MembershipType = e.MembershipType
        };
    }

    public Member? Get(Guid memberId)
    {
        var e = _db.Members.AsNoTracking()
            .FirstOrDefault(x => x.MemberId == memberId);

        return e is null ? null : new Member
        {
            MemberId = e.MemberId,
            Username = e.Username,
            PasswordHash = e.PasswordHash,
            MembershipType = e.MembershipType
        };
    }

    public void Add(Member member)
    {
        _db.Members.Add(new MemberEntity
        {
            MemberId = member.MemberId,
            Username = member.Username,
            PasswordHash = member.PasswordHash,
            MembershipType = member.MembershipType
        });

        try
        {
            _db.SaveChanges();
        }
        catch (DbUpdateException)
        {
            // SQLite unique constraint -> DbUpdateException
            throw new InvalidOperationException("UsernameTaken");
        }
    }
}
