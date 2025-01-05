using ProductManagement.Application.ViewModels;
using ProductManagement.Domain.Entities;

namespace ProductManagement.Application.Interfaces
{
    public interface IProductService
    {
        Task<Product> CreateProductAsync(ProductCreateVM productDto);
        Task<bool> DeleteProductAsync(Guid productId);
        Task<List<Product>> GetAllProductsAsync();
        Task<List<Product>> GetProductsByUserIdAsync(Guid userId);
        Task<List<Product>> GetAvailableProductsByUserIdAsync(Guid userId, bool isAvailable);
        Task<bool> ChangeAvailabilityAsync(Guid productId, bool isAvailable);
        Task<List<Product>> GetProductsCheaperThanAsync(decimal price);
        Task<List<Product>> GetProductsCheaperThanAndAvailableAsync(decimal price, bool isAvailable);
    }
}
