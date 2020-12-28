namespace Codex.Users.Api.Resources
{
    public class UserResource
    {
        public const string PASSWORD_MUST_BE_SET = nameof(PASSWORD_MUST_BE_SET);
        public const string LOGIN_MUST_BE_SET = nameof(LOGIN_MUST_BE_SET);
        public const string EMAIL_FORMAT_INVALID = nameof(EMAIL_FORMAT_INVALID);
        public const string USER_P0_ALREADY_EXISTS = nameof(USER_P0_ALREADY_EXISTS);
        public const string VALIDATION_CODE_IS_INVALID = nameof(VALIDATION_CODE_IS_INVALID);
        public const string VALIDATION_CODE_IS_EXPIRED = nameof(VALIDATION_CODE_IS_EXPIRED);
        public const string INVALID_LOGIN = nameof(INVALID_LOGIN);
        public const string USER_IS_DISABLED = nameof(USER_IS_DISABLED);
        public const string TENANT_ID_INSIDE_HEADER_MUST_BE_SAME_THAN_USER_TENANT_ID = nameof(TENANT_ID_INSIDE_HEADER_MUST_BE_SAME_THAN_USER_TENANT_ID);

    }
}
