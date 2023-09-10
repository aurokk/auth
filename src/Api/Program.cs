using System.Security.Cryptography.X509Certificates;
using Api;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;
var services = builder.Services;

// var applicationSettingsDto = new ApplicationConfigurationDto();
// configuration.Bind(applicationSettingsDto);
// var applicationSettings = new ApplicationConfiguration(
//     database: new DatabaseConfiguration(
//         connectionString: applicationSettingsDto.Database?.ConnectionString ?? throw new ApplicationException()
//     ),
//     authentication: new AuthenticationConfiguration(
//         authority: applicationSettingsDto.Authentication?.Authority ?? throw new ApplicationException()
//     ),
//     sendGrid: new SendGridConfiguration(
//         apiKey: applicationSettingsDto.SendGrid?.ApiKey ?? throw new ApplicationException()
//     )
// );
//
// services
//     .AddSingleton(_ => applicationSettings);

// services
//     .AddTransient<IEmailSender, EmailSender>();

services
    .AddControllersWithViews();

services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen(config =>
    {
        config.AddPrivateDoc();
        config.AddPublicDoc();
        config.CustomSchemaIds(s => s.FullName?.Replace("+", "."));
        config.DocInclusionPredicate((name, api) => name == api.GroupName);
    });

var secretBytes = await ResourcesHelper.GetResourceBytes("Secret.certificate.pfx");
var secret = new X509Certificate2(secretBytes);

services
    .Configure<ForwardedHeadersOptions>(
        options => options.ForwardedHeaders =
            ForwardedHeaders.XForwardedProto |
            ForwardedHeaders.XForwardedHost
    );

// overrides
services
    .AddSingleton<IConsentResponseMessageStore, ConsentResponseMessageStore>()
    .AddSingleton<ILoginResponseMessageStore, LoginResponseMessageStore>()
    .AddSingleton<ILoginRequestIdToResponseIdMessageStore, LoginRequestIdToResponseIdMessageStore>()
    .AddSingleton<ILoginResponseIdToRequestIdMessageStore, LoginResponseIdToRequestIdMessageStore>();

services
    .AddIdentityServer(options =>
    {
        options.Events.RaiseErrorEvents = true;
        options.Events.RaiseInformationEvents = true;
        options.Events.RaiseFailureEvents = true;
        options.Events.RaiseSuccessEvents = true;
        options.EmitStaticAudienceClaim = true;

        // todo: configure
        options.UserInteraction.LoginUrl = "http://localhost:20020/login";
        options.UserInteraction.LogoutUrl = "http://localhost:20020/logout";
        options.UserInteraction.ConsentUrl = "http://localhost:20020/consent";
    })
    .AddSigningCredential(secret)
    .AddInMemoryApiScopes(Config.ApiScopes)
    .AddInMemoryApiResources(Config.ApiResources)
    .AddInMemoryClients(Config.Clients)
    .AddInMemoryPersistedGrants();
// если нужно хранить конфигурацию в БД
// .AddConfigurationStore(options =>
// {
//     options.ConfigureDbContext = builder =>
//     {
//         builder
//             .UseNpgsql(
//                 applicationSettings.Database.ConnectionString,
//                 x => x.MigrationsAssembly("Migrations")
//             );
//     };
// })
// .AddOperationalStore(options =>
// {
//     options.ConfigureDbContext = builder =>
//     {
//         builder
//             .UseNpgsql(
//                 applicationSettings.Database.ConnectionString,
//                 x => x.MigrationsAssembly("Migrations")
//             );
//     };
// });
// .AddAspNetIdentity<ApplicationUser>()
// .AddResourceOwnerValidator<ResourceOwnerPasswordValidator<ApplicationUser>>()
// .AddExtensionGrantValidator<CustomSignInWithAppleGrantValidator>();

var application = builder.Build();

application
    .UseSwagger()
    .UseSwaggerUI(config =>
    {
        config.AddPrivateEndpoint();
        config.AddPublicEndpoint();
    });

application
    .UseForwardedHeaders()
    .UseStaticFiles()
    .UseRouting()
    .UseIdentityServer()
    .UseAuthentication()
    .UseAuthorization();

application
    .MapDefaultControllerRoute();

var mode = configuration.GetValue<string?>("MODE");
switch (mode)
{
    case "MIGRATOR":
    {
        // pgdc
        // using var scope = application.Services.CreateScope();
        // var pgc = scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>();
        // await pgc.Database.MigrateAsync();

        // cdc
        // var cc = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
        // await cc.Database.MigrateAsync();

        // aidc
        // var aidc = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        // await aidc.Database.MigrateAsync();

        return;
    }

    default:
    {
        await application.RunAsync();
        return;
    }
}