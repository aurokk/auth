using Api.Quickstart.Consent;
using IdentityServer4.Models;
using IdentityServer4.Services;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace Api.Api.Public.Consent;

[PublicAPI]
public sealed record GetOneResponse(
    GetOneResponse.ScopeDto[] ApiScopes,
    GetOneResponse.ClientDto Client,
    GetOneResponse.ScopeDto[] IdentityScopes)
{
    [PublicAPI]
    public sealed record ClientDto(string Name, string Description);

    [PublicAPI]
    public sealed record ScopeDto(string Name, string Description, string Value);
}

[PublicAPI]
public sealed record AcceptRequest(string ReturnUrl);

[PublicAPI]
public sealed record RejectRequest;

[ApiController]
[Route("api/public/consent")]
public class ConsentController : ControllerBase
{
    private readonly IIdentityServerInteractionService _interaction;

    public ConsentController(IIdentityServerInteractionService interaction) =>
        _interaction = interaction;

    [HttpGet]
    [Route("")]
    public async Task<IActionResult> GetOne(
        [FromQuery] string returnUrl,
        CancellationToken ct)
    {
        // тут надо костыльнуть
        var request = await _interaction.GetAuthorizationContextAsync(returnUrl);

        var apiScopes = request.ValidatedResources.Resources.ApiScopes
            .Select(s => new GetOneResponse.ScopeDto(
                Name: s.DisplayName ?? s.Name,
                Description: s.Description,
                Value: s.Name
            ))
            .ToArray();

        if (request.ValidatedResources.Resources.OfflineAccess)
        {
            apiScopes = apiScopes
                .Append(new GetOneResponse.ScopeDto(
                    Value: IdentityServer4.IdentityServerConstants.StandardScopes.OfflineAccess,
                    Name: ConsentOptions.OfflineAccessDisplayName,
                    Description: ConsentOptions.OfflineAccessDescription
                ))
                .ToArray();
        }

        var client = new GetOneResponse.ClientDto(
            Name: request.Client.ClientName ?? request.Client.ClientId,
            Description: request.Client.Description
        );

        var identityScopes = request.ValidatedResources.Resources.IdentityResources
            .Select(s => new GetOneResponse.ScopeDto(
                Name: s.DisplayName ?? s.Name,
                Description: s.Description,
                Value: s.Name
            ))
            .ToArray();

        var response = new GetOneResponse(
            ApiScopes: apiScopes,
            Client: client,
            IdentityScopes: identityScopes
        );

        return Ok(response);
    }

    [HttpPost]
    [Route("accept")]
    public async Task<IActionResult> Accept(
        [FromBody] AcceptRequest request,
        CancellationToken ct)
    {
        var context = await _interaction.GetAuthorizationContextAsync(returnUrl: request.ReturnUrl);
        var grantedConsent = new ConsentResponse
        {
            // Description = "No description",
            // Error = null,
            // ErrorDescription = null,
            RememberConsent = true,
            ScopesValuesConsented = new List<string>()
                .Concat(context.ValidatedResources.Resources.ApiScopes.Select(x => x.Name))
                .Concat(context.ValidatedResources.Resources.IdentityResources.Select(x => x.Name))
                .Append(IdentityServer4.IdentityServerConstants.StandardScopes.OfflineAccess)
                .ToArray(),
        };
        await _interaction.GrantConsentAsync(context, grantedConsent);

        return Ok();
    }

    [HttpPost]
    [Route("reject")]
    public async Task<IActionResult> Reject(
        [FromBody] RejectRequest request,
        CancellationToken ct)
    {
        await Task.CompletedTask;
        return Ok();
    }

    // private ConsentViewModel CreateConsentViewModel(
    //     ConsentInputModel model, string returnUrl,
    //     AuthorizationRequest request)
    // {
    //     var vm = new ConsentViewModel
    //     {
    //         RememberConsent = model?.RememberConsent ?? true,
    //         ScopesConsented = model?.ScopesConsented ?? Enumerable.Empty<string>(),
    //         Description = model?.Description,
    //
    //         ReturnUrl = returnUrl,
    //
    //         ClientName = request.Client.ClientName ?? request.Client.ClientId,
    //         ClientUrl = request.Client.ClientUri,
    //         ClientLogoUrl = request.Client.LogoUri,
    //         AllowRememberConsent = request.Client.AllowRememberConsent
    //     };
    //
    //     vm.IdentityScopes = request.ValidatedResources.Resources.IdentityResources.Select(x => CreateScopeViewModel(x, vm.ScopesConsented.Contains(x.Name) || model == null)).ToArray();
    //
    //     var apiScopes = new List<ScopeViewModel>();
    //     foreach(var parsedScope in request.ValidatedResources.ParsedScopes)
    //     {
    //         var apiScope = request.ValidatedResources.Resources.FindApiScope(parsedScope.ParsedName);
    //         if (apiScope != null)
    //         {
    //             var scopeVm = CreateScopeViewModel(parsedScope, apiScope, vm.ScopesConsented.Contains(parsedScope.RawValue) || model == null);
    //             apiScopes.Add(scopeVm);
    //         }
    //     }
    //     if (ConsentOptions.EnableOfflineAccess && request.ValidatedResources.Resources.OfflineAccess)
    //     {
    //         apiScopes.Add(GetOfflineAccessScope(vm.ScopesConsented.Contains(IdentityServer4.IdentityServerConstants.StandardScopes.OfflineAccess) || model == null));
    //     }
    //     vm.ApiScopes = apiScopes;
    //
    //     return vm;
    // }

    // return new ScopeViewModel
    // {
    //     Value = identity.Name,
    //     DisplayName = identity.DisplayName ?? identity.Name,
    //     Description = identity.Description,
    //     Emphasize = identity.Emphasize,
    //     Required = identity.Required,
    //     Checked = check || identity.Required
    // };
}