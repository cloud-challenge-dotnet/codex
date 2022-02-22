using System.Diagnostics.CodeAnalysis;

namespace Codex.Users.Api.Resources;

[ExcludeFromCodeCoverage]
public class UserResource
{
    public const string PasswordMustBeSet = nameof(PasswordMustBeSet);
    public const string LoginMustBeSet = nameof(LoginMustBeSet);
    public const string EmailFormatInvalid = nameof(EmailFormatInvalid);
    public const string UserP0AlreadyExists = nameof(UserP0AlreadyExists);
    public const string ValidationCodeIsInvalid = nameof(ValidationCodeIsInvalid);
    public const string ValidationCodeIsExpired = nameof(ValidationCodeIsExpired);
    public const string InvalidLogin = nameof(InvalidLogin);
    public const string UserIsDisabled = nameof(UserIsDisabled);
    public const string TenantIdInsideHeaderMustBeSameThanUserTenantId = nameof(TenantIdInsideHeaderMustBeSameThanUserTenantId);

}