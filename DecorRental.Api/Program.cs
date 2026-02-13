using System.Text;
using DecorRental.Api.Middleware;
using DecorRental.Api.Security;
using DecorRental.Api.Validators;
using DecorRental.Application.UseCases.CancelReservation;
using DecorRental.Application.UseCases.CreateKit;
using DecorRental.Application.UseCases.GetKitById;
using DecorRental.Application.UseCases.GetKits;
using DecorRental.Application.UseCases.ReserveKit;
using DecorRental.Domain.Repositories;
using DecorRental.Infrastructure.Persistence;
using DecorRental.Infrastructure.Repositories;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
        ValidateJwtSecurityConfiguration(jwtOptions);

        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey))
        };
        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();

                var correlationId = CorrelationIdMiddleware.ResolveCorrelationId(context.HttpContext);
                var problemDetails = new ProblemDetails
                {
                    Type = "https://httpstatuses.com/401",
                    Title = "Unauthorized",
                    Detail = "A valid access token is required to access this resource.",
                    Status = StatusCodes.Status401Unauthorized,
                    Instance = context.HttpContext.Request.Path
                };
                problemDetails.Extensions["code"] = "unauthorized";
                problemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
                problemDetails.Extensions["correlationId"] = correlationId;

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/problem+json";
                await context.Response.WriteAsJsonAsync(problemDetails);
            },
            OnForbidden = async context =>
            {
                var correlationId = CorrelationIdMiddleware.ResolveCorrelationId(context.HttpContext);
                var problemDetails = new ProblemDetails
                {
                    Type = "https://httpstatuses.com/403",
                    Title = "Forbidden",
                    Detail = "Your user does not have permission to access this resource.",
                    Status = StatusCodes.Status403Forbidden,
                    Instance = context.HttpContext.Request.Path
                };
                problemDetails.Extensions["code"] = "forbidden";
                problemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
                problemDetails.Extensions["correlationId"] = correlationId;

                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/problem+json";
                await context.Response.WriteAsJsonAsync(problemDetails);
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthorizationPolicies.ReadKits, policy =>
        policy.RequireRole("Viewer", "Manager", "Admin"));

    options.AddPolicy(AuthorizationPolicies.ManageKits, policy =>
        policy.RequireRole("Manager", "Admin"));
});

builder.Services.AddScoped<IAuthenticationService, JwtAuthenticationService>();

builder.Services.AddDbContext<DecorRentalDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IKitRepository, EfKitRepository>();
builder.Services.AddScoped<CreateKitHandler>();
builder.Services.AddScoped<GetKitByIdHandler>();
builder.Services.AddScoped<GetKitsHandler>();
builder.Services.AddScoped<CancelReservationHandler>();
builder.Services.AddScoped<ReserveKitHandler>();

builder.Services.AddValidatorsFromAssemblyContaining<CreateKitRequestValidator>();
builder.Services.AddFluentValidationAutoValidation();

builder.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var validationProblemDetails = new ValidationProblemDetails(context.ModelState)
            {
                Type = "https://httpstatuses.com/400",
                Title = "Validation failed.",
                Detail = "One or more request fields are invalid.",
                Status = StatusCodes.Status400BadRequest,
                Instance = context.HttpContext.Request.Path
            };

            var correlationId = CorrelationIdMiddleware.ResolveCorrelationId(context.HttpContext);
            validationProblemDetails.Extensions["code"] = "validation_error";
            validationProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
            validationProblemDetails.Extensions["correlationId"] = correlationId;

            return new BadRequestObjectResult(validationProblemDetails)
            {
                ContentTypes = { "application/problem+json" }
            };
        };
    });

builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "DecorRental API",
        Version = "v1"
    });

    var bearerSecurityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = JwtBearerDefaults.AuthenticationScheme
        }
    };

    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, bearerSecurityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [bearerSecurityScheme] = []
    });
});

builder.Services.AddHealthChecks()
    .AddDbContextCheck<DecorRentalDbContext>("database");

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DecorRentalDbContext>();
    dbContext.Database.Migrate();
}

app.UseHttpMetrics();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");
app.MapMetrics("/metrics");

app.Run();

static void ValidateJwtSecurityConfiguration(JwtOptions jwtOptions)
{
    if (string.IsNullOrWhiteSpace(jwtOptions.Issuer) ||
        string.IsNullOrWhiteSpace(jwtOptions.Audience) ||
        string.IsNullOrWhiteSpace(jwtOptions.SigningKey))
    {
        throw new InvalidOperationException(
            $"Missing JWT security configuration. Set '{JwtOptions.SectionName}:Issuer', '{JwtOptions.SectionName}:Audience' and '{JwtOptions.SectionName}:SigningKey'.");
    }

    if (jwtOptions.SigningKey.Length < 32)
    {
        throw new InvalidOperationException("JWT signing key must have at least 32 characters.");
    }
}

public partial class Program;
