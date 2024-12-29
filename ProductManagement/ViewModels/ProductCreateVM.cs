namespace ProductManagement.ViewModels
{
    public class ProductCreateVM
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public Guid CreatorUserId { get; set; }
    }
}
