using System.Security.Claims;
using System.Web;
using Api.EmailSender;
using Identity;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Api.EmailConfirmation;

[PublicAPI]
public record ConfirmEmailRequest(string Email, string Token);

[Authorize("User")]
[ApiController]
[Route("api/emailconfirmation")]
public class EmailConfirmationController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender _emailSender;

    public EmailConfirmationController(UserManager<ApplicationUser> userManager, IEmailSender emailSender)
    {
        _userManager = userManager;
        _emailSender = emailSender;
    }

    [ProducesResponseType(200)]
    [Route("sendlink")]
    [HttpPost]
    public async Task<IActionResult> SendLink(CancellationToken ct)
    {
        var userIdClaim = User.Claims.SingleOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userIdClaim.Value);
        if (user is null)
            return BadRequest();

        if (user.Email == null)
            return BadRequest();

        if (user.EmailConfirmed)
            return BadRequest();

        var token = HttpUtility.UrlEncode(await _userManager.GenerateEmailConfirmationTokenAsync(user));
        var requestHost = new Uri($"{Request.Scheme}://{Request.Host}");
        var confirmationLink = new Uri(requestHost, $"api/emailconfirmation/confirm?token={token}&email={user.Email}");
        await _emailSender.SendEmail(
            toEmail: user.Email,
            subject: "Email confirmation",
            message: $"Click <a href=\"{confirmationLink.AbsoluteUri}\">here</a> to confirm your email address.",
            ct: ct
        );

        return Ok();
    }

    [AllowAnonymous]
    [ProducesResponseType(200)]
    [Route("confirm")]
    [HttpGet]
    public async Task<IActionResult> Confirm([FromQuery] ConfirmEmailRequest request, CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            throw new ApplicationException();

        if (user.Email == null)
            return BadRequest();

        if (user.EmailConfirmed)
            return BadRequest();

        var result = await _userManager.ConfirmEmailAsync(user, request.Token);
        var message = result.Succeeded ? "Success" : "Error";

        return Ok(message);
    }
}