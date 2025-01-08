using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductManagement.Application.Interfaces;
using ProductManagement.Application.ViewModels;
using System.ComponentModel.DataAnnotations;
using System.Configuration;

namespace ProductManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }
     

        [HttpPost("/create-product")]
        public async Task<IActionResult> CreateProduct([FromBody] ProductCreateVM productDto, IValidator<ProductCreateVM> validator)
        {
           var validationResult =validator.Validate(productDto);
            if (!validationResult.IsValid) 
            {
                var problemDetails = new HttpValidationProblemDetails(validationResult.ToDictionary())
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation failed",
                    Detail = "One or more validation errors occurred"
                };
                return BadRequest(problemDetails);
            }

            var result = await _productService.CreateProductAsync(productDto);
            if (result == null)
                return BadRequest("Failed to create product");
            return Ok(result);

        }
        [HttpPut("/delete-product")]
        public async Task<IActionResult> DeleteProduct([FromBody] ProductSoftDeleteVM productDto, IValidator<ProductSoftDeleteVM> validator)
        {
            var validationResult = validator.Validate(productDto);
            if (!validationResult.IsValid)
            {
                var problemDetails = new HttpValidationProblemDetails(validationResult.ToDictionary())
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation failed",
                    Detail = "One or more validation errors occurred"
                };
                return BadRequest(problemDetails);
            }

            var result = await _productService.DeleteProductAsync(productDto.Id);

            if (!result)
                return BadRequest("Product doesn't exist or is already deleted");

            return Ok("Product was soft-deleted");
        }

        [HttpGet("get-all-products")]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await _productService.GetAllProductsAsync();

            if (products == null || products.Count == 0)
                return NotFound("No products were found");

            return Ok(products);
        }

        [HttpGet("/get-products-by-userId")]
        public async Task<IActionResult> GetAllProductsAssignedToUserId(Guid userId)
        {
            if (userId == Guid.Empty)
                return BadRequest("Provide a valid user ID");

            var products = await _productService.GetProductsByUserIdAsync(userId);

            if (products == null || products.Count == 0)
                return NotFound("This user doesn't have any products assigned");

            return Ok(products);

        }
        [HttpGet("/get-available-products-by-userId")]
        public async Task<IActionResult> GetAllAvilableProductsAssignedToUserId(Guid userId, bool isAvailable)
        {
            if(userId == Guid.Empty)
                return BadRequest("Provide a valid user ID");

            var products = await _productService.GetAvailableProductsByUserIdAsync(userId, isAvailable);

            if (products == null || products.Count == 0)
                return NotFound("No available products for this user");

            return Ok(products);

        }
        [HttpPut("/change-availability")]
        public async Task<IActionResult> ChangeAvailabilityOfProduct(bool isAvailable, Guid productId)
        {
            if (productId == Guid.Empty)
                return BadRequest("Provide a valid product ID");

            var result = await _productService.ChangeAvailabilityAsync(productId, isAvailable);

            if (!result)
                return BadRequest("Couldn't find product or update availability");

            return Ok("Product availability updated successfully");
        }
        [HttpGet("/get-products-cheaper-than")]
        public async Task<IActionResult> GetCheaperProductsThan(decimal price)
        {
            var products = await _productService.GetProductsCheaperThanAsync(price);

            if (products == null || products.Count == 0)
                return NotFound("No cheaper products were found");

            return Ok(products);
        }
        [HttpGet("/get-products-cheaper-than-and-Available")]
        public async Task<IActionResult> GetCheaperProductsThanIsAvilable(decimal price, bool isAvailable)
        {
            var products = await _productService.GetProductsCheaperThanAndAvailableAsync(price, isAvailable);

            if (products == null || products.Count == 0)
                return NotFound("No such products were found");

            return Ok(products);
        }
    }


}
