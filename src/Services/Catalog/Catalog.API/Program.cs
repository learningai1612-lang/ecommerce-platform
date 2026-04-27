using Catalog.API.Middleware;
using Catalog.Application.Common.Interfaces;
using Catalog.Application.Products.Commands.CreateProduct;
using Catalog.Infrastructure.Persistence;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Catalog API", Version = "v1" });
});

// Add DbContext
builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("CatalogDb"),
        b => b.MigrationsAssembly(typeof(CatalogDbContext).Assembly.FullName)));

builder.Services.AddScoped<ICatalogDbContext>(provider => provider.GetRequiredService<CatalogDbContext>());

// Add MediatR with validation pipeline
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(CreateProductCommand).Assembly);
    cfg.AddOpenBehavior(typeof(Catalog.Application.Common.Behaviors.ValidationBehavior<,>));
});

// Add FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(typeof(CreateProductCommand).Assembly);

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseExceptionHandling();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<CatalogDbContext>();
        
        if (app.Environment.IsDevelopment())
        {
            await context.Database.MigrateAsync();
            await CatalogDbContextSeed.SeedAsync(context);
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}

app.Run();
