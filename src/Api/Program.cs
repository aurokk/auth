using System.Security.Cryptography.X509Certificates;
using Api;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

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
    .AddCors(options =>
    {
        var configuration = builder.Configuration;
        var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? throw new Exception();
        options
            .AddDefaultPolicy(policy =>
                policy
                    .WithOrigins(origins)
                    .AllowCredentials()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
            );
    });

services
    .Configure<ForwardedHeadersOptions>(
        options => options.ForwardedHeaders =
            ForwardedHeaders.XForwardedProto |
            ForwardedHeaders.XForwardedHost
    );

services
    .Configure<ApiBehaviorOptions>(
        options => options.SuppressInferBindingSourcesForParameters = true
    );

// overrides
services
    .AddSingleton<IConsentResponseMessageStore, ConsentResponseMessageStore>()
    .AddSingleton<ILoginResponseMessageStore, LoginResponseMessageStore>()
    .AddSingleton<ILoginRequestIdToResponseIdMessageStore, LoginRequestIdToResponseIdMessageStore>()
    .AddSingleton<ILoginResponseIdToRequestIdMessageStore, LoginResponseIdToRequestIdMessageStore>();

services
    .AddHealthChecks();

services
    .AddIdentityServer(options =>
    {
        options.Events.RaiseErrorEvents = true;
        options.Events.RaiseInformationEvents = true;
        options.Events.RaiseFailureEvents = true;
        options.Events.RaiseSuccessEvents = true;

        options.EmitStaticAudienceClaim = true;

        var configuration = builder.Configuration;
        var identityBaseUrl = configuration.GetValue<string>("IdentityUI:BaseUrl") ?? "http://empty";
        var identityBaseUri = new Uri(identityBaseUrl);
        options.UserInteraction.LoginUrl = new Uri(identityBaseUri, "login").AbsoluteUri;
        options.UserInteraction.LogoutUrl = new Uri(identityBaseUri, "logout").AbsoluteUri;
        options.UserInteraction.ConsentUrl = new Uri(identityBaseUri, "consent").AbsoluteUri;
    })
    .AddSigningCredential(secret)
    // .AddInMemoryApiScopes(Config.ApiScopes)
    // .AddInMemoryApiResources(Config.ApiResources)
    // .AddInMemoryClients(Config.Clients)
    // .AddInMemoryPersistedGrants()
    .AddConfigurationStore(options =>
    {
        options.ConfigureDbContext = dbContextBuilder =>
        {
            var configuration = builder.Configuration;
            var connectionString = configuration.GetValue<string>("Database:ConnectionString");
            dbContextBuilder.UseNpgsql(connectionString, b => b.MigrationsAssembly("Migrations"));
        };
    })
    .AddOperationalStore(options =>
    {
        options.ConfigureDbContext = dbContextBuilder =>
        {
            var configuration = builder.Configuration;
            var connectionString = configuration.GetValue<string>("Database:ConnectionString");
            dbContextBuilder.UseNpgsql(connectionString, b => b.MigrationsAssembly("Migrations"));
        };
    });
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
    .UseCors()
    .UseIdentityServer()
    .UseAuthentication()
    .UseAuthorization();

application
    .MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = healthCheck => healthCheck.Tags.Contains("ready"),
    });

application
    .MapHealthChecks("/health/live", new HealthCheckOptions
    {
        Predicate = _ => false,
    });

application
    .MapDefaultControllerRoute();

{
    var configuration = builder.Configuration;
    var mode = configuration.GetValue<string?>("MODE");
    switch (mode)
    {
        case "MIGRATOR":
        {
            using var scope = application.Services.CreateScope();
            var pgc = scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>();
            await pgc.Database.MigrateAsync();
            var cc = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
            await cc.Database.MigrateAsync();
            return;
        }

        default:
        {
            await application.RunAsync();
            return;
        }
    }
}