using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace ServiceCRM.Services.Identity;

public class LocalizedIdentityErrorDescriber : IdentityErrorDescriber
{
    private readonly IStringLocalizer _localizer;

    public LocalizedIdentityErrorDescriber(IStringLocalizerFactory factory)
    {
        var type = typeof(LocalizedIdentityErrorDescriber);
        _localizer = factory.Create("IdentityErrorMessages", typeof(LocalizedIdentityErrorDescriber).Assembly.FullName!);
    }

    public override IdentityError PasswordTooShort(int length)
    {
        return new IdentityError
        {
            Code = nameof(PasswordTooShort),
            Description = string.Format(_localizer[nameof(PasswordTooShort)], length)
        };
    }

    public override IdentityError PasswordRequiresDigit()
    {
        return new IdentityError
        {
            Code = nameof(PasswordRequiresDigit),
            Description = _localizer[nameof(PasswordRequiresDigit)]
        };
    }

    public override IdentityError PasswordRequiresLower()
    {
        return new IdentityError
        {
            Code = nameof(PasswordRequiresLower),
            Description = _localizer[nameof(PasswordRequiresLower)]
        };
    }

    public override IdentityError DuplicateUserName(string userName)
    {
        return new IdentityError
        {
            Code = nameof(DuplicateUserName),
            Description = string.Format(_localizer[nameof(DuplicateUserName)], userName)
        };
    }
}
