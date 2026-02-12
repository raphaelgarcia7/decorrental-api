using DecorRental.Api.Middleware;
using DecorRental.Api.Validators;
using DecorRental.Application.UseCases.CancelReservation;
using DecorRental.Application.UseCases.CreateKit;
using DecorRental.Application.UseCases.GetKitById;
using DecorRental.Application.UseCases.GetKits;
using DecorRental.Application.UseCases.ReserveKit;
using DecorRental.Domain.Repositories;
using FluentValidation;
using FluentValidation.AspNetCore;
using DecorRental.Infrastructure.Persistence;
using DecorRental.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<DecorRentalDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<IKitRepository, EfKitRepository>();

// Use cases
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
            var errors = context.ModelState
                .Values
                .SelectMany(value => value.Errors)
                .Select(error => error.ErrorMessage)
                .Where(message => !string.IsNullOrWhiteSpace(message))
                .ToArray();

            var message = errors.Length > 0
                ? string.Join(" ", errors)
                : "Validation failed.";

            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(
                new DecorRental.Api.Contracts.ErrorResponse("validation_error", message));
        };
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

public partial class Program;
