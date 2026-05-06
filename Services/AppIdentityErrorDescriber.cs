using Microsoft.AspNetCore.Identity;

namespace App.Services;

public class AppIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError DuplicateRoleName(string role)
    {
        var er = base.DuplicateRoleName(role);
        return new IdentityError()
        {
            Code = er.Code,
            Description = $"Vai trò {role} đã tồn tại."
        };
    }
    public override IdentityError DuplicateEmail(string email)
    {
        var er = base.DuplicateEmail(email);
        return new IdentityError()
        {
            Code = er.Code,
            Description = $"Email {email} đã tồn tại."
        };
    }
    public override IdentityError DuplicateUserName(string userName)
    {
        var er = base.DuplicateUserName(userName);
        return new IdentityError()
        {
            Code = er.Code,
            Description = $"Tên đăng nhập {userName} đã tồn tại."
        };
    }

}