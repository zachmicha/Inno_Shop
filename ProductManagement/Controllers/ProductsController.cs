using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using ProductManagement.Data;
using ProductManagement.Models;
using ProductManagement.ViewModels;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace ProductManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly ProductDbContext _dbContext;
        public ProductsController(HttpClient httpClient, ProductDbContext context)
        {
                _httpClient = httpClient;
            _dbContext = context;
        }
        //error 500 Idea was to retrieve JWT token to use it  as a authorization on this microservice too.

        //[HttpPost("login")]
        //public async Task<IActionResult> Login([FromBody] UserLoginVM userDto)
        //{
        //    if (userDto == null || string.IsNullOrWhiteSpace(userDto.Email) || string.IsNullOrWhiteSpace(userDto.Password))
        //    {
        //        return BadRequest("Email and password are required.");
        //    }

        //    // Define the UserManagement API endpoint
        //    var userManagementApiUrl = "https://localhost:32771/api/User/login";

        //    // Serialize the request body
        //    var content = new StringContent(JsonSerializer.Serialize(userDto), Encoding.UTF8, "application/json");

        //    // Send POST request to UserManagement API
        //   // Throws error here when trying to post, refuses to connect to the server for whatever reason
   
        //        var response = await _httpClient.PostAsync(userManagementApiUrl, content);

        //    if (response.IsSuccessStatusCode)
        //    {
        //        var responseBody = await response.Content.ReadAsStringAsync();
        //        return Ok(JsonSerializer.Deserialize<object>(responseBody)); // Return token to the client
        //    }

        //    // Return error response if the login fails
        //    return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        //}

        [HttpPost("/create-product")]
        // can't add authorization cause Login post method doesn't work, and JWT token is not available cause of that, and making other JWT token just for this mock api service
        //misses the point, since in production app you would store one token per session and refresh it if needed or whatever other required options
        //[Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> CreateProduct([FromBody] ProductCreateVM productDto)
        {
            if (productDto == null)
                return BadRequest("Invalid data model");
            var product = new Product
            {
                Name = productDto.Name,
                Description = productDto.Description,
                Price = productDto.Price,
                CreatorUserId = productDto.CreatorUserId,
                CreatedAt = DateTime.UtcNow,
                IsAvailable = true,
                IsDeleted = false,
            };
            _dbContext.Add(product);
            await _dbContext.SaveChangesAsync();

            return Ok(product);
        }
        [HttpPut("/delete-product")]
        public async Task<IActionResult> DeleteProduct([FromBody] ProductSoftDeleteVM productDto)
        {
            if (productDto==null)
            {
                return BadRequest("Invalid data");
            }
           var product= await _dbContext.Products.FindAsync(productDto.Id);
            if (product==null || product.IsDeleted==true)
            {
                return BadRequest("Product doesn't exist");
            }
            product.IsDeleted = true;
            await _dbContext.SaveChangesAsync();
            return Ok("(Soft)Deleted the product");
        }

        [HttpGet("get-all-products")]
        public async Task<IActionResult> GetAllProducts()
        {
            var productsCollection = await _dbContext.Products.Where(p=>p.IsDeleted==false).ToListAsync();
            if (productsCollection ==null)
            {
                return BadRequest("No products were found");
            }
            return Ok(productsCollection);
        }

        [HttpGet("/get-products-by-userId")]
        public async Task<IActionResult> GetAllProductsAssignedToUserId(Guid userId)
        {
            if (userId==null)
            {
                return BadRequest("Provide user id");
            }
            var productsCollection = await _dbContext.Products.Where(p => p.CreatorUserId == userId && p.IsDeleted ==false).ToListAsync();
            if (productsCollection ==null)
            {
                return BadRequest("This user doesn't have any products assigned to him");
            }
            return Ok(productsCollection);

        }
        [HttpGet("/get-available-products-by-userId")]
        public async Task<IActionResult> GetAllAvilableProductsAssignedToUserId(Guid userId, bool isAvailable)
        {
            if (userId == null || isAvailable ==null)
            {
                return BadRequest("Provide user id, and if product is in stock");
            }
            var productsCollection = await _dbContext.Products.Where(p => p.CreatorUserId == userId && p.IsDeleted == false && p.IsAvailable==isAvailable).ToListAsync();
            if (productsCollection == null)
            {
                return BadRequest("This user doesn't have any products assigned to him or no available products");
            }
            return Ok(productsCollection);

        }
        [HttpPut("/change-availability")]
        public async Task<IActionResult> ChangeAvailabilityOfProduct(bool isAvailable, Guid productId)
        {
            if (isAvailable==null || productId == null)
            {
                return BadRequest("Provide correct values");
            }
            var product = await _dbContext.Products.FindAsync(productId);
            if (product==null)
            {
                return BadRequest("Couldn't find product");
            }
            product.IsAvailable=isAvailable;
            await _dbContext.SaveChangesAsync();
            return Ok($"{product.Name} availability is {product.IsAvailable}");
        }
        [HttpGet("/get-products-cheaper-than")]
        public async Task<IActionResult> GetCheaperProductsThan(decimal price)
        {
            if (price==null)
            {
                return BadRequest("Provide valid value");
            }
            var productsCollection = await _dbContext.Products.Where(p=> p.Price <price && p.IsDeleted==false).ToListAsync();
            if (productsCollection==null)
            {
                return BadRequest("No such cheap products were found");
            }
            return Ok(productsCollection);
        }
        [HttpGet("/get-products-cheaper-than-and-Available")]
        public async Task<IActionResult> GetCheaperProductsThan(decimal price, bool isAvailable)
        {
            if (price == null || isAvailable==null)
            {
                return BadRequest("Provide valid value");
            }
            var productsCollection = await _dbContext.Products.Where(p => p.Price < price && p.IsAvailable ==isAvailable && p.IsDeleted == false).ToListAsync();
            if (productsCollection == null)
            {
                return BadRequest("No such cheap products were found");
            }
            return Ok(productsCollection);
        }
    }


}
