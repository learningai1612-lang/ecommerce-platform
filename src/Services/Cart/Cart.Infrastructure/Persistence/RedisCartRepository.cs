using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;
using Cart.Application.Common.Interfaces;
using Cart.Domain.Entities;

namespace Cart.Infrastructure.Persistence;

public class RedisCartRepository : ICartRepository
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisCartRepository> _logger;
    private readonly TimeSpan _cartExpiration = TimeSpan.FromHours(24); // 24 hour TTL
    private const string KeyPrefix = "cart:";

    public RedisCartRepository(
        IConnectionMultiplexer redis,
        ILogger<RedisCartRepository> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<ShoppingCart?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var key = GetKey(userId);
            var value = await db.StringGetAsync(key);

            if (value.IsNullOrEmpty)
            {
                _logger.LogDebug("Cart not found in Redis for user {UserId}", userId);
                return null;
            }

            var cart = JsonSerializer.Deserialize<ShoppingCart>(value!);
            _logger.LogDebug("Cart retrieved from Redis for user {UserId}", userId);

            return cart;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cart from Redis for user {UserId}", userId);
            throw;
        }
    }

    public async Task SaveAsync(ShoppingCart cart, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var key = GetKey(cart.UserId);
            var value = JsonSerializer.Serialize(cart);

            // Set with TTL for automatic cleanup
            var success = await db.StringSetAsync(key, value, _cartExpiration);

            if (success)
            {
                _logger.LogDebug(
                    "Cart {CartId} saved to Redis for user {UserId}. Items: {ItemCount}, Total: {TotalAmount:C}, TTL: {ExpirationHours}h",
                    cart.Id,
                    cart.UserId,
                    cart.Items.Count,
                    cart.TotalAmount,
                    _cartExpiration.TotalHours);
            }
            else
            {
                _logger.LogWarning("Failed to save cart {CartId} to Redis for user {UserId}", cart.Id, cart.UserId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving cart to Redis for user {UserId}", cart.UserId);
            throw;
        }
    }

    public async Task DeleteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var key = GetKey(userId);
            var deleted = await db.KeyDeleteAsync(key);

            if (deleted)
            {
                _logger.LogDebug("Cart deleted from Redis for user {UserId}", userId);
            }
            else
            {
                _logger.LogDebug("No cart found to delete for user {UserId}", userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting cart from Redis for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var key = GetKey(userId);
            return await db.KeyExistsAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking cart existence in Redis for user {UserId}", userId);
            throw;
        }
    }

    private static string GetKey(Guid userId) => $"{KeyPrefix}{userId}";
}
