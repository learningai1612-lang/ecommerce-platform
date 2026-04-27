using FluentValidation;
using StackExchange.Redis;
using Cart.Application.Common.Interfaces;
using Cart.Application.Carts.Commands.AddItemToCart;
using Cart.Infrastructure.Persistence;
using Cart.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Cart API",
        Version = "v1",
        Description = "High-performance Cart microservice using Redis for fast read/write operations"
    });
});

// Configure Redis
var redisConnectionString = builder.Configuration.GetConnectionString("Redis") 
    ?? "localhost:6379";

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = ConfigurationOptions.Parse(redisConnectionString);
    configuration.AbortOnConnectFail = false;
    configuration.ConnectRetry = 3;
    configuration.ConnectTimeout = 5000;
    return ConnectionMultiplexer.Connect(configuration);
});

// Add Repository
builder.Services.AddScoped<ICartRepository, RedisCartRepository>();

// Add MediatR with validation pipeline
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(AddItemToCartCommand).Assembly);
    cfg.AddOpenBehavior(typeof(Cart.Application.Common.Behaviors.ValidationBehavior<,>));
});

// Add FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(AddItemToCartCommand).Assembly);

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
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cart API v1");
    });
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

// Test Redis connection on startup
try
{
    var redis = app.Services.GetRequiredService<IConnectionMultiplexer>();
    var db = redis.GetDatabase();
    db.Ping();
    app.Logger.LogInformation("✓ Redis connection successful");
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "✗ Redis connection failed. Ensure Redis is running on {ConnectionString}", redisConnectionString);
}

app.Run();
