
namespace ProductManagement.Domain.Entities;

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }        
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
    public Guid CreatorUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsDeleted { get; set; } // For soft delete
    
}
