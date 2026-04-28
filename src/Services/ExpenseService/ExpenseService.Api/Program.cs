using System.Text;
using System.Text.Json.Serialization;
using ExpenseService.Api;
using ExpenseService.Application;
using ExpenseService.Infrastructure;
using ExpenseService.Infrastructure.Auth;
using ExpenseService.Infrastructure.Contexts;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, _, configuration) =>
{
    configuration
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft.AspNetCore.DataProtection", LogEventLevel.Error)
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console();
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "ExpenseService API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "JWT Bearer token - format: **Bearer {token}**",
        In = Microsoft.OpenApi.ParameterLocation.Header,
        Type = Microsoft.OpenApi.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    options.AddSecurityRequirement(doc => new Microsoft.OpenApi.OpenApiSecurityRequirement
    {
        { new Microsoft.OpenApi.OpenApiSecuritySchemeReference("Bearer", doc), new List<string>() }
    });
});
builder.Services.AddHealthChecks();

builder.Services.AddExpenseApplication();
builder.Services.AddExpenseInfrastructure(builder.Configuration);

var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
var localLoginEnabled = builder.Configuration.GetValue("Authentication:EnableLocalLogin", true);
const string smartJwtScheme = "SmartJwt";
const string localJwtScheme = "LocalJwt";
const string externalJwtScheme = "ExternalJwt";
var authenticationBuilder = builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = smartJwtScheme;
        options.DefaultAuthenticateScheme = smartJwtScheme;
        options.DefaultChallengeScheme = smartJwtScheme;
    })
    .AddPolicyScheme(smartJwtScheme, null, options =>
    {
        options.ForwardDefaultSelector = context =>
        {
            if (string.IsNullOrWhiteSpace(jwtOptions.Authority))
            {
                return localJwtScheme;
            }

            var authorization = context.Request.Headers.Authorization.ToString();
            if (!authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return localJwtScheme;
            }

            var token = authorization["Bearer ".Length..].Trim();
            var header = token.Split('.', 2)[0];
            try
            {
                var headerJson = Encoding.UTF8.GetString(Base64UrlEncoder.DecodeBytes(header));
                using var document = System.Text.Json.JsonDocument.Parse(headerJson);
                var algorithm = document.RootElement.GetProperty("alg").GetString();
                return algorithm?.StartsWith("RS", StringComparison.OrdinalIgnoreCase) == true
                    ? externalJwtScheme
                    : localJwtScheme;
            }
            catch
            {
                return localJwtScheme;
            }
        };
    })
    .AddJwtBearer(localJwtScheme, options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            RoleClaimType = "role",
            NameClaimType = "UserId",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret))
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                if (!localLoginEnabled &&
                    context.Principal?.Claims.Any(x => x.Type == "role" && x.Value == "Service") != true)
                {
                    context.Fail("Local user JWTs are disabled. Use Keycloak access tokens.");
                }

                return Task.CompletedTask;
            }
        };
    });

if (!string.IsNullOrWhiteSpace(jwtOptions.Authority))
{
    var validExternalIssuers = new HashSet<string>(
        new[]
        {
            jwtOptions.Authority.TrimEnd('/'),
            jwtOptions.PublicAuthority?.TrimEnd('/')
        }.Where(x => !string.IsNullOrWhiteSpace(x))!,
        StringComparer.OrdinalIgnoreCase);

    authenticationBuilder.AddJwtBearer(externalJwtScheme, options =>
    {
        options.MapInboundClaims = false;
        options.Authority = jwtOptions.Authority;
        options.RequireHttpsMetadata = jwtOptions.RequireHttpsMetadata;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuers = validExternalIssuers,
            IssuerValidator = (issuer, _, _) =>
            {
                var normalizedIssuer = issuer?.TrimEnd('/');
                if (!string.IsNullOrWhiteSpace(normalizedIssuer) &&
                    validExternalIssuers.Contains(normalizedIssuer))
                {
                    return normalizedIssuer;
                }

                throw new SecurityTokenInvalidIssuerException($"Invalid issuer '{issuer}'.");
            },
            ValidAudience = jwtOptions.Audience,
            RoleClaimType = "role",
            NameClaimType = "UserId"
        };
    });
}

var authSchemes = string.IsNullOrWhiteSpace(jwtOptions.Authority)
    ? new[] { localJwtScheme }
    : new[] { smartJwtScheme };
builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder(authSchemes)
        .RequireAuthenticatedUser()
        .Build();
});

var otlpEndpoint = builder.Configuration["OpenTelemetry:OtlpEndpoint"];
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("expense-service"))
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation(opts => opts.RecordException = true)
            .AddHttpClientInstrumentation()
            .AddSource("ExpenseService.Messaging");
        if (!string.IsNullOrWhiteSpace(otlpEndpoint))
        {
            tracing.AddOtlpExporter(opt => opt.Endpoint = new Uri(otlpEndpoint));
        }
    });

var app = builder.Build();

app.UseMiddleware<ApiExceptionMiddleware>();
app.UseMiddleware<CorrelationMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
if (!string.IsNullOrWhiteSpace(app.Configuration["HTTPS_PORT"]) ||
    !string.IsNullOrWhiteSpace(app.Configuration["ASPNETCORE_HTTPS_PORT"]))
{
    app.UseHttpsRedirection();
}
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/health");
app.MapControllers();

app.Run();

public partial class Program { }
