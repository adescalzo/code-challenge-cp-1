using System.Text;
using Asp.Versioning;
using EmployeeChallenge.Api.Core;
using EmployeeChallenge.Api.Infrastructure.Configuration;
using EmployeeChallenge.Infrastructure;
using EmployeeChallenge.Infrastructure.Data;
using EmployeeChallenge.Infrastructure.Extensions;
using EmployeeChallenge.Infrastructure.Mediator;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Configure Problem Details
// Configure DbContext with In-Memory Database
// Register IClock for ApplicationDbContext
builder.Services.AddSingleton<IClock, Clock>();

// Register the seeder
builder.Services.AddScoped<IDbSeeder, ApplicationDbContextSeeder>();
builder.Services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
{
    options.UseInMemoryDatabase("EmployeeChallengeDb");

    options.UseAsyncSeeding(async (context, _, cancellationToken) =>
    {
        var seeder = serviceProvider.GetRequiredService<IDbSeeder>();
        await seeder.SeedAsync(cancellationToken).ConfigureAwait(false);
    });
});

// Register DbContext for UnitOfWork
builder.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

// Configure error handling and problem details
builder.Services.AddProblemDetailsConfiguration();
builder.Services.AddExceptionHandling();

// Configure infrastructure
builder.Services.AddResponseCompressionConfiguration();
builder.Services.AddHealthChecks();

// Configure API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1);
    options.ReportApiVersions = true;
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'V";
    options.SubstituteApiVersionInUrl = true;
});

// Configure JWT Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();

builder.Services.AddServices();

// Register UnitOfWork
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register Repositories
builder.Services.AddRepositories();

// Configure Health Checks
builder.Services.AddHealthChecks();

// Configure Endpoints
builder.Services.AddEndpoints(typeof(Program).Assembly);
builder.Services.AddMediator(typeof(Program).Assembly);

// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerConfiguration(
    title:"Employee Challenge API",
    version: "v1",
    description: "A RESTful API for managing employees with supervisor relationships"
);

var app = builder.Build();

// Ensure the database is created (this will trigger seeding automatically)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.EnsureCreatedAsync().ConfigureAwait(false);
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Employee Challenge API v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}

app.UseHttpsRedirection();
app.MapEndpoints(CreateVersionGroup(app));
app.MapHealthChecks("/health");
app.UseAuthentication();
app.UseAuthorization();

await app.RunAsync().ConfigureAwait(false);

return;

static RouteGroupBuilder CreateVersionGroup(WebApplication app)
{
    var apiVersionSet = app.NewApiVersionSet()
        .HasApiVersion(new ApiVersion(1))
        .ReportApiVersions()
        .Build();

    var versionedGroup = app
        .MapGroup("api/v{version:apiVersion}")
        .WithApiVersionSet(apiVersionSet);

    return versionedGroup;
}
