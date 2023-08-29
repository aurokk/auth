using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity;

namespace Api;

[UsedImplicitly]
public class ResourceOwnerPasswordValidator<TUser> : IResourceOwnerPasswordValidator
    where TUser : class
{
    private readonly SignInManager<TUser> _signInManager;
    private readonly UserManager<TUser> _userManager;
    private readonly ILogger<ResourceOwnerPasswordValidator<TUser>> _logger;

    public ResourceOwnerPasswordValidator(
        UserManager<TUser> userManager,
        SignInManager<TUser> signInManager,
        ILogger<ResourceOwnerPasswordValidator<TUser>> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    public virtual async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
    {
        var user = await _userManager.FindByEmailAsync(context.UserName);
        if (user != null)
        {
            var result = await _signInManager.CheckPasswordSignInAsync(user, context.Password, true);
            if (result.Succeeded)
            {
                var sub = await _userManager.GetUserIdAsync(user);

                _logger.LogInformation("Credentials validated for username: {username}", context.UserName);

                context.Result = new GrantValidationResult(sub, OidcConstants.AuthenticationMethods.Password);
                return;
            }
            else if (result.IsLockedOut)
            {
                _logger.LogInformation("Authentication failed for username: {username}, reason: locked out",
                    context.UserName);
            }
            else if (result.IsNotAllowed)
            {
                _logger.LogInformation("Authentication failed for username: {username}, reason: not allowed",
                    context.UserName);
            }
            else
            {
                _logger.LogInformation("Authentication failed for username: {username}, reason: invalid credentials",
                    context.UserName);
            }
        }
        else
        {
            _logger.LogInformation("No user found matching username: {username}", context.UserName);
        }

        context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant);
    }
}