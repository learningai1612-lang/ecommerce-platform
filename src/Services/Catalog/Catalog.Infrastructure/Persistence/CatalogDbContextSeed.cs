using Catalog.Domain.Entities;

namespace Catalog.Infrastructure.Persistence;

public static class CatalogDbContextSeed
{
    public static async Task SeedAsync(CatalogDbContext context)
    {

        // Seed Categories
        if (!context.Categories.Any())
        {
            var categories = new List<Category>
            {
                new Category { Id = Guid.NewGuid(), Name = "Electronics", Description = "Electronic devices and accessories", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Category { Id = Guid.NewGuid(), Name = "Clothing", Description = "Apparel and fashion items", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Category { Id = Guid.NewGuid(), Name = "Books", Description = "Books and reading materials", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Category { Id = Guid.NewGuid(), Name = "Home", Description = "Home appliances and furniture", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Category { Id = Guid.NewGuid(), Name = "Sports", Description = "Sports equipment and accessories", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Category { Id = Guid.NewGuid(), Name = "Toys", Description = "Toys and games for kids", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Category { Id = Guid.NewGuid(), Name = "Beauty", Description = "Beauty and personal care products", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            };
            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();
        }

        // Seed Products and Images
        if (!context.Products.Any())
        {
            var categories = context.Categories.ToList();
            var electronicsCategory = categories.First(c => c.Name == "Electronics");
            var clothingCategory = categories.First(c => c.Name == "Clothing");
            var booksCategory = categories.First(c => c.Name == "Books");
            var homeCategory = categories.First(c => c.Name == "Home");
            var sportsCategory = categories.First(c => c.Name == "Sports");
            var toysCategory = categories.First(c => c.Name == "Toys");
            var beautyCategory = categories.First(c => c.Name == "Beauty");

            var products = new List<Product>
            {
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Laptop",
                    Description = "High-performance laptop for professionals",
                    Price = 1299.99m,
                    Stock = 50,
                    Sku = "LAP-001",
                    CategoryId = electronicsCategory.Id,
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Images = new List<ProductImage>
                    {
                        new ProductImage { Id = Guid.NewGuid(), ImageUrl = "https://images.unsplash.com/photo-1517336714731-489689fd1ca8", AltText = "Modern Laptop", IsPrimary = true, DisplayOrder = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                        new ProductImage { Id = Guid.NewGuid(), ImageUrl = "https://images.unsplash.com/photo-1519389950473-47ba0277781c", AltText = "Laptop Side View", IsPrimary = false, DisplayOrder = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
                    }
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Wireless Mouse",
                    Description = "Ergonomic wireless mouse",
                    Price = 29.99m,
                    Stock = 200,
                    Sku = "MOU-001",
                    CategoryId = electronicsCategory.Id,
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Images = new List<ProductImage>
                    {
                        new ProductImage { Id = Guid.NewGuid(), ImageUrl = "https://images.unsplash.com/photo-1519125323398-675f0ddb6308", AltText = "Wireless Mouse", IsPrimary = true, DisplayOrder = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                        new ProductImage { Id = Guid.NewGuid(), ImageUrl = "https://images.unsplash.com/photo-1465101046530-73398c7f28ca", AltText = "Mouse in Hand", IsPrimary = false, DisplayOrder = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
                    }
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Men's T-Shirt",
                    Description = "100% cotton, comfortable fit",
                    Price = 19.99m,
                    Stock = 120,
                    Sku = "TSH-001",
                    CategoryId = clothingCategory.Id,
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Images = new List<ProductImage>
                    {
                        new ProductImage { Id = Guid.NewGuid(), ImageUrl = "https://images.unsplash.com/photo-1512436991641-6745cdb1723f", AltText = "Men's T-Shirt", IsPrimary = true, DisplayOrder = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
                    }
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Fiction Novel",
                    Description = "Bestselling fiction novel for all ages",
                    Price = 14.99m,
                    Stock = 80,
                    Sku = "BOK-001",
                    CategoryId = booksCategory.Id,
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Images = new List<ProductImage>
                    {
                        new ProductImage { Id = Guid.NewGuid(), ImageUrl = "https://images.unsplash.com/photo-1512820790803-83ca734da794", AltText = "Book Cover", IsPrimary = true, DisplayOrder = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
                    }
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Blender",
                    Description = "Multi-speed kitchen blender",
                    Price = 49.99m,
                    Stock = 60,
                    Sku = "HOM-001",
                    CategoryId = homeCategory.Id,
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Images = new List<ProductImage>
                    {
                        new ProductImage { Id = Guid.NewGuid(), ImageUrl = "https://images.unsplash.com/photo-1504674900247-0877df9cc836", AltText = "Kitchen Blender", IsPrimary = true, DisplayOrder = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
                    }
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Yoga Mat",
                    Description = "Non-slip yoga mat for all exercises",
                    Price = 24.99m,
                    Stock = 90,
                    Sku = "SPT-001",
                    CategoryId = sportsCategory.Id,
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Images = new List<ProductImage>
                    {
                        new ProductImage { Id = Guid.NewGuid(), ImageUrl = "https://images.unsplash.com/photo-1519864600265-abb23847ef2c", AltText = "Yoga Mat", IsPrimary = true, DisplayOrder = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
                    }
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Building Blocks Set",
                    Description = "Creative building blocks for kids (100 pcs)",
                    Price = 34.99m,
                    Stock = 70,
                    Sku = "TOY-001",
                    CategoryId = toysCategory.Id,
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Images = new List<ProductImage>
                    {
                        new ProductImage { Id = Guid.NewGuid(), ImageUrl = "https://images.unsplash.com/photo-1506744038136-46273834b3fb", AltText = "Building Blocks", IsPrimary = true, DisplayOrder = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
                    }
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Facial Cleanser",
                    Description = "Gentle foaming facial cleanser",
                    Price = 12.99m,
                    Stock = 110,
                    Sku = "BEA-001",
                    CategoryId = beautyCategory.Id,
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Images = new List<ProductImage>
                    {
                        new ProductImage { Id = Guid.NewGuid(), ImageUrl = "https://images.unsplash.com/photo-1515378791036-0648a3ef77b2", AltText = "Facial Cleanser", IsPrimary = true, DisplayOrder = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
                    }
                }
            };

            context.Products.AddRange(products);
            await context.SaveChangesAsync();
        }
    }
}
