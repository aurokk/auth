using System.Security.Claims;
using Identity;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Api.Users;

[PublicAPI]
public record SignUpRequest(string Email, string Password);

[PublicAPI]
public record SignUpResponse(bool IsSuccess, string[] Errors);

[PublicAPI]
public sealed record GetMeResponse(string AccountId, string[] Providers, GetMeResponse.EmailDto? Email)
{
    [PublicAPI]
    public sealed record EmailDto(string Email, bool IsConfirmed);
}

[PublicAPI]
public sealed record SetEmailRequest(string Email);

[PublicAPI]
public record DeleteRequest;

[PublicAPI]
public record DeleteResponse(bool IsSuccess, string[] Errors);

[Authorize("User")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UsersController(UserManager<ApplicationUser> userManager) =>
        _userManager = userManager;

    [AllowAnonymous]
    [ProducesResponseType(typeof(SignUpRequest), 200)]
    [Route("api/signup")] // obsolete
    [Route("api/users/signup")]
    [HttpPost]
    public async Task<IActionResult> SignUp([FromBody] SignUpRequest request, CancellationToken ct)
    {
        var user = new ApplicationUser
        {
            UserName = Guid.NewGuid().ToString(),
            Email = request.Email,
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        switch (createResult)
        {
            case { Succeeded: true }:
            {
                // send email

                var response = new SignUpResponse(
                    IsSuccess: true,
                    Errors: Array.Empty<string>()
                );
                return Ok(response);
            }

            default:
            {
                var response = new SignUpResponse(
                    IsSuccess: false,
                    Errors: createResult.Errors
                        .Select(MapError)
                        .ToArray()
                );
                return Ok(response);
            }
        }
    }

    [ProducesResponseType(typeof(GetMeResponse), 200)]
    [Route("api/users/getme")]
    [HttpGet]
    public async Task<IActionResult> GetMe(CancellationToken ct)
    {
        var userIdClaim = User.Claims.SingleOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userIdClaim.Value);
        if (user is null)
            return BadRequest();

        var logins = await _userManager.GetLoginsAsync(user);

        var hasGoogle = logins.Any(x => x.LoginProvider == "Google");
        var hasApple = logins.Any(x => x.LoginProvider == "Apple");
        var hasEmail = user.Email != null;

        var providers = new[]
            {
                hasGoogle ? "google" : null,
                hasApple ? "apple" : null,
                hasEmail ? "email" : null,
            }
            .Where(x => x != null)
            .OfType<string>()
            .ToArray();

        var response = new GetMeResponse(
            AccountId: user.Id,
            Providers: providers,
            Email: hasEmail
                ? new GetMeResponse.EmailDto(
                    Email: user.Email ?? throw new ApplicationException(),
                    IsConfirmed: user.EmailConfirmed
                )
                : null
        );

        return Ok(response);
    }

    [ProducesResponseType(200)]
    [Route("api/users/setemail")]
    [HttpPost]
    public async Task<IActionResult> SetEmail(SetEmailRequest request, CancellationToken ct)
    {
        var userIdClaim = User.Claims.SingleOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userIdClaim.Value);
        if (user is null)
            return BadRequest();

        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest();

        var result = await _userManager.SetEmailAsync(user, request.Email);
        if (!result.Succeeded)
            return BadRequest();

        return Ok();
    }

    [ProducesResponseType(typeof(DeleteResponse), 200)]
    [Route("api/deleteaccount")] // obsolete
    [Route("api/users/delete")]
    [HttpPost]
    public async Task<IActionResult> Delete([FromBody] DeleteRequest request, CancellationToken ct)
    {
        var userIdClaim = User.Claims.SingleOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userIdClaim.Value);
        if (user is null)
        {
            var response = new DeleteResponse(
                IsSuccess: false,
                Errors: new[] { "USER_NOT_FOUND" }
            );
            return Ok(response);
        }

        var result = await _userManager.DeleteAsync(user);
        switch (result.Succeeded)
        {
            case true:
            {
                var response = new DeleteResponse(
                    IsSuccess: true,
                    Errors: Array.Empty<string>()
                );
                return Ok(response);
            }
            default:
            {
                var response =
                    new DeleteResponse(
                        IsSuccess: false,
                        Errors: result.Errors
                            .Select(x => x.Code)
                            .ToArray()
                    );
                return Ok(response);
            }
        }
    }

    // @formatter:off
    private static string MapError(IdentityError error) =>
        error.Code switch
        {
            "DuplicateEmail"                  => ErrorCode.EMAIL_ALREADY_EXISTS,
            "DuplicateUserName"               => ErrorCode.EMAIL_ALREADY_EXISTS,

            "PasswordTooShort"                => ErrorCode.PASSWORD_TOO_SHORT,
            "PasswordRequiresNonAlphanumeric" => ErrorCode.PASSWORD_REQUIRES_NON_ALPHANUMERIC,
            "PasswordRequiresDigit"           => ErrorCode.PASSWORD_REQUIRES_DIGIT,
            "PasswordRequiresLower"           => ErrorCode.PASSWORD_REQUIRES_LOWER,
            "PasswordRequiresUpper"           => ErrorCode.PASSWORD_REQUIRES_UPPER,
            "PasswordRequiresUniqueChars"     => ErrorCode.PASSWORD_REQUIRES_UNIQUE_CHARS,

                                            _ => ErrorCode.UNKNOWN_ERROR,
        };
    // @formatter:on
}