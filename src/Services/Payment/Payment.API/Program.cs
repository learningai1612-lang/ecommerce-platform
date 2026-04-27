using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Payment.Application.Common.Interfaces;
using Payment.Application.Payments.Commands.ProcessPayment;
using Payment.Infrastructure.Persistence;
using Payment.Infrastructure.Services;
using Payment.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Payment API",
        Version = "v1",
        Description = "Event-driven Payment microservice with idempotency and simulated payment gateway"
    });
});

// Add DbContext
builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly(typeof(PaymentDbContext).Assembly.FullName)));

builder.Services.AddScoped<IPaymentDbContext>(provider => provider.GetRequiredService<PaymentDbContext>());

// Add Services
builder.Services.AddScoped<IPaymentGateway, SimulatedPaymentGateway>();
builder.Services.AddSingleton<IEventPublisher, InMemoryEventPublisher>();

// Add MediatR with validation pipeline
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(ProcessPaymentCommand).Assembly);
    cfg.AddOpenBehavior(typeof(Payment.Application.Common.Behaviors.ValidationBehavior<,>));
});

// Add FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(ProcessPaymentCommand).Assembly);

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
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Payment API v1");
    });
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
