namespace IdentityServer4.Models;

public class LoginResponse
{
    public bool IsSuccess { get; set; }
    public string? SubjectId { get; set; }
}