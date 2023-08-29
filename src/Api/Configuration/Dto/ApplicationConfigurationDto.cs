using JetBrains.Annotations;

namespace Api.Configuration.Dto;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed class ApplicationConfigurationDto
{
    public DatabaseConfigurationDto? Database { get; set; }
    public AuthenticationConfigurationDto? Authentication { get; set; }
    public SendGridConfigurationDto? SendGrid { get; set; }
}