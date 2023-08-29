using JetBrains.Annotations;

namespace Api.Configuration.Dto;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed class DatabaseConfigurationDto
{
    public string? ConnectionString { get; set; }
}