using FluentValidation;
using Inventory.API.Middleware;
using Inventory.Application.Common.Interfaces;
using Inventory.Application.Inventory.Commands.AddStock;
using Inventory.Infrastructure.Persistence;
using Inventory.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost;Database=InventoryDb;Trusted_Connection=True;TrustServerCertificate=True;";

// Database
builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseSqlServer(connectionString,
        b => b.MigrationsAssembly("Inventory.Infrastructure")));

// Application services
builder.Services.AddScoped<IInventoryDbContext>(provider =>
    provider.GetRequiredService<InventoryDbContext>());

// Event publishing
builder.Services.AddSingleton<IEventPublisher, InMemoryEventPublisher>();

// MediatR with validation behavior
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(AddStockCommand).Assembly);
    cfg.AddOpenBehavior(typeof(Inventory.Application.Common.Behaviors.ValidationBehavior<,>));
});

// FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(AddStockCommand).Assembly);

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Inventory API", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
