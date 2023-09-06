using System.Security.Cryptography.X509Certificates;
using Api;
using Api.Configuration;
using Api.Configuration.Dto;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;
var services = builder.Services;

var applicationSettingsDto = new ApplicationConfigurationDto();
configuration.Bind(applicationSettingsDto);
var applicationSettings = new ApplicationConfiguration(
    database: new DatabaseConfiguration(
        connectionString: applicationSettingsDto.Database?.ConnectionString ?? throw new ApplicationException()
    ),
    authentication: new AuthenticationConfiguration(
        authority: applicationSettingsDto.Authentication?.Authority ?? throw new ApplicationException()
    ),
    sendGrid: new SendGridConfiguration(
        apiKey: applicationSettingsDto.SendGrid?.ApiKey ?? throw new ApplicationException()
    )
);

services
    .AddSingleton(_ => applicationSettings);

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

// services
//     .AddDbContext<ApplicationDbContext>((sp, options) =>
//     {
//         options.UseNpgsql(
//             applicationSettings.Database.ConnectionString,
//             x => x.MigrationsAssembly("Migrations")
//         );
//     });

// services
//     .Configure<IdentityOptions>(options =>
//     {
//         options.User.RequireUniqueEmail = true;
//         // options.SignIn.RequireConfirmedEmail = false;
//         // options.SignIn.RequireConfirmedAccount = false;
//         // options.Password.RequireNonAlphanumeric = false;
//     })
//     .AddIdentity<ApplicationUser, IdentityRole>()
//     .AddEntityFrameworkStores<ApplicationDbContext>()
//     .AddDefaultTokenProviders();

// services
//     .AddAuthorization(options =>
//     {
//         options.AddPolicy("User", policy =>
//         {
//             policy.AddAuthenticationSchemes("Bearer");
//             policy.RequireAuthenticatedUser();
//             policy.RequireClaim("scope", "words-api");
//         });
//     });

// services
//     .AddAuthentication()
//     .AddJwtBearer("Bearer", options =>
//     {
//         options.Authority = applicationSettings.Authentication.Authority;
//         options.Audience = "words-api";
//         options.RequireHttpsMetadata = false;
//         options.TokenValidationParameters = new TokenValidationParameters
//         {
//             ValidateAudience = true,
//             ValidateLifetime = true,
//             ClockSkew = TimeSpan.Zero,
//         };
//         options.Events = new JwtBearerEvents
//         {
//             OnMessageReceived = context =>
//             {
//                 var accessToken = context.Request.Query["access_token"];
//                 switch (accessToken)
//                 {
//                     case { } when !string.IsNullOrEmpty(accessToken):
//                     {
//                         break;
//                     }
//
//                     default:
//                     {
//                         return Task.CompletedTask;
//                     }
//                 }
//
//                 var path = context.HttpContext.Request.Path;
//                 switch (path)
//                 {
//                     case { } when path.StartsWithSegments("/api/users/logins/link"):
//                     case { } when path.StartsWithSegments("/api/users/logins/linkcallback"):
//                     {
//                         context.Token = accessToken;
//                         return Task.CompletedTask;
//                     }
//
//                     default:
//                     {
//                         return Task.CompletedTask;
//                     }
//                 }
//             }
//         };
// });
// .AddGoogle("Google", options =>
// {
//     options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
//     options.ClientId = "698791982574-12p020e5hhr2nik63lebpcu60fp7g3g9.apps.googleusercontent.com";
//     options.ClientSecret = "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";
// });

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
    .AddInMemoryPersistedGrants()
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
    // })
    .AddTestUsers(Config.Users);
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