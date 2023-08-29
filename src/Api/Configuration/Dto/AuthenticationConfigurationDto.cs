using JetBrains.Annotations;

namespace Api.Configuration.Dto;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed class AuthenticationConfigurationDto
{
    public string? Authority { get; set; }
}