using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProductManagement.Application.Interfaces;
using ProductManagement.Application.ViewModels;
using ProductManagement.Domain.Entities;
using ProductManagement.Infrastructure.Databases;
namespace ProductManagement.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly ProductDbContext _dbContext;
        public ProductService(ProductDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> ChangeAvailabilityAsync(Guid productId, bool isAvailable)
        {
            var product = await _dbContext.Products.FindAsync(productId);
            if (product == null) return false;

            if (product.IsAvailable==isAvailable) return false;

            product.IsAvailable = isAvailable;
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<Product> CreateProductAsync(ProductCreateVM productDto)
        {
            var product = new Product
            {
                Name = productDto.Name,
                Description = productDto.Description,
                Price = productDto.Price,
                CreatorUserId = productDto.CreatorUserId,
                CreatedAt = DateTime.UtcNow,
                IsAvailable = true,
                IsDeleted = false
            };
            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync();
            return product;
        }

        public async Task<bool> DeleteProductAsync(Guid productId)
        {
            var product = await _dbContext.Products.FindAsync(productId);
            if (product == null || product.IsDeleted==true)
            {
                return false;
            }
            product.IsDeleted = true;
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            return await _dbContext.Products.Where(p=> p.IsDeleted==false).ToListAsync();
        }

        public async Task<List<Product>> GetAvailableProductsByUserIdAsync(Guid userId, bool isAvailable)
        {
            return await _dbContext.Products.Where(p => p.IsDeleted == false && p.CreatorUserId==userId && p.IsAvailable == isAvailable).ToListAsync();

        }

        public async Task<List<Product>> GetProductsByUserIdAsync(Guid userId)
        {
            return await _dbContext.Products.Where(p => p.CreatorUserId == userId && !p.IsDeleted).ToListAsync();
        }

        public async Task<List<Product>> GetProductsCheaperThanAndAvailableAsync(decimal price, bool isAvailable)
        {
            return await _dbContext.Products.Where(p => p.Price < price && p.IsAvailable == isAvailable && !p.IsDeleted).ToListAsync();
        }

        public async Task<List<Product>> GetProductsCheaperThanAsync(decimal price)
        {
            return await _dbContext.Products.Where(p => p.Price < price && !p.IsDeleted).ToListAsync();
        }
               
    }
}

