using System.Security.Cryptography.X509Certificates;
using Api;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Storage.DbContexts;
using IdentityServer4.EntityFramework.Storage.Stores;
using IdentityServer4.Storage.Stores;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var webHost = builder.WebHost;
var services = builder.Services;
var configuration = builder.Configuration;

{
    var mode = configuration.GetValue<string?>("Mode");
    switch (mode)
    {
        case "WEB":
        {
            var privateHttpPort = configuration.GetValue<int?>("PrivateApi:HttpPort");
            var privateHttpsPort = configuration.GetValue<int?>("PrivateApi:HttpsPort");
            var privateApiPorts = new List<string>();
            if (privateHttpPort != null) privateApiPorts.Add($"http://+:{privateHttpPort}");
            if (privateHttpsPort != null) privateApiPorts.Add($"https://+:{privateHttpsPort}");
            if (!privateApiPorts.Any()) throw new Exception();

            var publicHttpPort = configuration.GetValue<int?>("PublicApi:HttpPort");
            var publicHttpsPort = configuration.GetValue<int?>("PublicApi:HttpsPort");
            var publicApiPorts = new List<string>();
            if (publicHttpPort != null) publicApiPorts.Add($"http://+:{publicHttpPort}");
            if (publicHttpsPort != null) publicApiPorts.Add($"https://+:{publicHttpsPort}");
            if (!publicApiPorts.Any()) throw new Exception();

            var allPorts = privateApiPorts.Concat(publicApiPorts).ToArray();
            var allPortsUnique = privateApiPorts.Concat(publicApiPorts).ToHashSet();
            if (allPorts.Length != allPortsUnique.Count) throw new Exception();

            webHost.UseUrls(string.Join(";", allPorts));
            break;
        }
    }
}


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
        var origins = configuration.GetSection("Cors:Origins").Get<string[]>() ?? Array.Empty<string>();
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
    .AddSingleton<IConsentResponseMessageStore, ConsentResponseMessageStore>();

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

        var cosmoBaseUrl = configuration.GetValue<string>("Cosmo:BaseUrl") ?? "http://empty";
        var cosmoBaseUri = new Uri(cosmoBaseUrl);
        options.UserInteraction.LoginUrl = new Uri(cosmoBaseUri, "login").AbsoluteUri;
        options.UserInteraction.LogoutUrl = new Uri(cosmoBaseUri, "logout").AbsoluteUri;
        options.UserInteraction.ConsentUrl = new Uri(cosmoBaseUri, "consent").AbsoluteUri;
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
            var connectionString = configuration.GetValue<string>("Database:ConnectionString");
            dbContextBuilder.UseNpgsql(connectionString, b => b.MigrationsAssembly("Migrations"));
        };
    })
    .AddOperationalStore(options =>
    {
        options.ConfigureDbContext = dbContextBuilder =>
        {
            var connectionString = configuration.GetValue<string>("Database:ConnectionString");
            dbContextBuilder.UseNpgsql(connectionString, b => b.MigrationsAssembly("Migrations"));
        };
    });

services
    .AddScoped<IStore, Store>()
    .AddScoped<IAuthorizeRequest2Store, AuthorizeRequest2Store>()
    .AddScoped<ILoginRequestStore, LoginRequestStore>()
    .AddScoped<ILoginResponseStore, LoginResponseStore>()
    .AddScoped<IConsentRequest2Store, ConsentRequestStore>()
    .AddScoped<IConsentResponse2Store, ConsentResponseStore>()
    .AddDbContext<OperationalDbContext>(dbContextBuilder =>
    {
        var connectionString = configuration.GetValue<string>("Database:ConnectionString");
        dbContextBuilder.UseNpgsql(connectionString, b => b.MigrationsAssembly("Migrations"));
    });

// .AddAspNetIdentity<ApplicationUser>()
// .AddResourceOwnerValidator<ResourceOwnerPasswordValidator<ApplicationUser>>()
// .AddExtensionGrantValidator<CustomSignInWithAppleGrantValidator>();

var application = builder.Build();

application
    .MapWhen(
        context =>
            (context.Connection.LocalPort == context.RequestServices.GetRequiredService<IConfiguration>()
                 .GetValue<int>("PublicApi:HttpPort") ||
             context.Connection.LocalPort == context.RequestServices.GetRequiredService<IConfiguration>()
                 .GetValue<int>("PublicApi:HttpsPort")) &&
            (context.Request.Path.StartsWithSegments("/api/public") ||
             context.Request.Path.StartsWithSegments("/connect") ||
             context.Request.Path.StartsWithSegments("/.well-known")),
        publicApplication => publicApplication
            .UseForwardedHeaders()
            .UseStaticFiles()
            .UseRouting()
            .UseCors()
            .UseIdentityServer()
            .UseAuthentication()
            .UseAuthorization()
            .UseEndpoints(endpoints => endpoints.MapControllers())
    );

application
    .MapWhen(
        context =>
            (context.Connection.LocalPort == context.RequestServices.GetRequiredService<IConfiguration>()
                 .GetValue<int>("PrivateApi:HttpPort") ||
             context.Connection.LocalPort == context.RequestServices.GetRequiredService<IConfiguration>()
                 .GetValue<int>("PrivateApi:HttpsPort")) &&
            (context.Request.Path.StartsWithSegments("/api/private") ||
             context.Request.Path.StartsWithSegments("/health") ||
             context.Request.Path.StartsWithSegments("/swagger")),
        privateApplication => privateApplication
            .UseSwagger()
            .UseSwaggerUI(config =>
            {
                config.AddPrivateEndpoint();
                config.AddPublicEndpoint();
            })
            .UseRouting()
            .UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
                {
                    Predicate = healthCheck => healthCheck.Tags.Contains("ready"),
                });
                endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
                {
                    Predicate = _ => false,
                });
                endpoints.MapControllers();
            })
    );

{
    var mode = configuration.GetValue<string?>("Mode");
    switch (mode)
    {
        case "MIGRATOR":
        {
            using var scope = application.Services.CreateScope();
            var pgc = scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>();
            await pgc.Database.MigrateAsync();
            var cc = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
            await cc.Database.MigrateAsync();
            var oc = scope.ServiceProvider.GetRequiredService<OperationalDbContext>();
            await oc.Database.MigrateAsync();
            return;
        }

        case "WEB":
        {
            await application.RunAsync();
            return;
        }
    }
}