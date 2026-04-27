using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Order.Application.Common.Interfaces;
using Order.Application.Orders.Commands.CreateOrder;
using Order.Infrastructure.Persistence;
using Order.API.Middleware;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Order API",
        Version = "v1",
        Description = "Order microservice API for ecommerce platform following Clean Architecture and DDD principles"
    });
});

// Add DbContext
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly(typeof(OrderDbContext).Assembly.FullName)));

builder.Services.AddScoped<IOrderDbContext>(provider => provider.GetRequiredService<OrderDbContext>());

// Add MediatR with validation pipeline
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateOrderCommand).Assembly);
    cfg.AddOpenBehavior(typeof(Order.Application.Common.Behaviors.ValidationBehavior<,>));
});

// Add FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(CreateOrderCommand).Assembly);

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order API v1");
    });
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
