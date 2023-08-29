namespace Api;

public static class ErrorCode
{
    // username
    public const string EMAIL_ALREADY_EXISTS = "EMAIL_ALREADY_EXISTS";

    // password
    public const string PASSWORD_TOO_SHORT = "PASSWORD_TOO_SHORT";
    public const string PASSWORD_REQUIRES_NON_ALPHANUMERIC = "PASSWORD_REQUIRES_NON_ALPHANUMERIC";
    public const string PASSWORD_REQUIRES_DIGIT = "PASSWORD_REQUIRES_DIGIT";
    public const string PASSWORD_REQUIRES_LOWER = "PASSWORD_REQUIRES_LOWER";
    public const string PASSWORD_REQUIRES_UPPER = "PASSWORD_REQUIRES_UPPER";
    public const string PASSWORD_REQUIRES_UNIQUE_CHARS = "PASSWORD_REQUIRES_UNIQUE_CHARS";

    // other
    public const string UNKNOWN_ERROR = "UNKNOWN_ERROR";
}