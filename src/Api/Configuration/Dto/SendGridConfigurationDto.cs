using JetBrains.Annotations;

namespace Api.Configuration.Dto;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed class SendGridConfigurationDto
{
    public string? ApiKey { get; set; }
}