using Cart.Domain.Entities;

namespace Cart.Application.Common.Interfaces;

public interface ICartRepository
{
    /// <summary>
    /// Get cart by user ID
    /// </summary>
    Task<ShoppingCart?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Save or update cart
    /// </summary>
    Task SaveAsync(ShoppingCart cart, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete cart
    /// </summary>
    Task DeleteAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if cart exists
    /// </summary>
    Task<bool> ExistsAsync(Guid userId, CancellationToken cancellationToken = default);
}
