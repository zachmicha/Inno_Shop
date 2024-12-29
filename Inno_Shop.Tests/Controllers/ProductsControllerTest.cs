using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductManagement.Controllers;
using ProductManagement.Data;
using ProductManagement.Models;
using ProductManagement.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Inno_Shop.Tests.Controllers
{
    public class ProductsControllerTest
    {
        private readonly HttpClient _httpClient;
       
        public ProductsControllerTest()
        {
            _httpClient = A.Fake<HttpClient>();
            
        }
        private async Task<ProductDbContext> GetDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<ProductDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var databaseContext = new ProductDbContext(options);
            databaseContext.Database.EnsureCreated();
            if (await databaseContext.Products.CountAsync()<=0)
            {
                for (int i = 0; i < 3; i++)
                {
                    databaseContext.Products.Add(new Product
                    {
                        Id= new Guid($"51b489dd-cbe6-4328-8deb-08dd2792671{i}"),
                        Name = $"Product{i}",
                        Description = "None",
                        Price = 10+i,
                        IsAvailable = true,
                        CreatorUserId = new Guid("23ba54c2-e930-4189-9775-1b52b3a0ae79"),
                        CreatedAt = DateTime.Now,
                        IsDeleted = false
                    });
                }
                await databaseContext.SaveChangesAsync();
            }
            return databaseContext;
        }
        [Fact]
        public async void ProductController_GetAllProducts_ReturnsOk()
        {
            //Arrange
            
            var dbContext = await GetDatabaseContext();
            var productsRepository = new ProductsController(_httpClient, dbContext);

            //Act
            var result = await productsRepository.GetAllProducts();

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType(typeof(OkObjectResult));
           
        }

        [Fact]
        public async void ProductController_GetAllAvilableProductsAssignedTOUserId_ReturnsOk()
        {
            var dbContext = await GetDatabaseContext();
            var productsRepository = new ProductsController(_httpClient, dbContext);

            //Act
            var result = await productsRepository.GetAllAvilableProductsAssignedToUserId(new Guid("23ba54c2-e930-4189-9775-1b52b3a0ae79"),true);

            result.Should().NotBeNull();            
            result.Should().BeOfType(typeof(OkObjectResult));           
        }
        [Fact]
        public async void ProductController_GetAllAvilableProductsAssignedTOUserId_ReturnsBadRequest()
        {
            var userId = Guid.Empty;
            var dbContext = await GetDatabaseContext();
            var productsRepository = new ProductsController(_httpClient, dbContext);

            //Act
            var result = await productsRepository.GetAllAvilableProductsAssignedToUserId(userId, false);

            result.Should().NotBeNull();
            result.Should().BeOfType(typeof(BadRequestObjectResult));
        }

        [Fact]
        public async void ProductController_ChangeAvailabilityOfProduct_ReturnsOk()
        {
            //Arrange
            var dbContext = await GetDatabaseContext();
            var productRepository = new ProductsController(_httpClient, dbContext);                
            //Act
            var result = await productRepository.ChangeAvailabilityOfProduct(false, new Guid($"51b489dd-cbe6-4328-8deb-08dd27926710"));
            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType(typeof(OkObjectResult));

        }
        [Fact]
        public async void ProductController_ChangeAvailabilityOfProduct_ReturnsBadRequest()
        {
            //Arrange
            var dbContext = await GetDatabaseContext();
            var productRepository = new ProductsController(_httpClient, dbContext);
            var sGuid = Guid.Empty;
            //Act
            var result = await productRepository.ChangeAvailabilityOfProduct(false, sGuid);
            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType(typeof(BadRequestObjectResult));

        }
        [Fact]
        public async void ProductController_CreateProduct_ReturnsOk()
        {
            //Arrange
            var dbContext = await GetDatabaseContext();
            var productRepository = new ProductsController(_httpClient, dbContext);
            var product = new ProductCreateVM
            {
                Name = $"ProductNew",
                Description = "None",
                Price = 10,
                CreatorUserId = new Guid("23ba54c2-e930-4189-9775-1b52b3a0ae79"),
            };
            //Act
            var result = await productRepository.CreateProduct(product);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType(typeof(OkObjectResult));
        }
        [Fact]
        public async void ProductController_CreateProduct_ReturnsBadRequest()
        {
            //Arrange
            var dbContext = await GetDatabaseContext();
            var productRepository = new ProductsController(_httpClient, dbContext);
            ProductCreateVM product = null;
            //Act
            var result = await productRepository.CreateProduct(product);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType(typeof(BadRequestObjectResult));
        }
        [Fact]
        public async void ProductController_DeleteProduct_ReturnsOk()
        {
            var dbContext = await GetDatabaseContext();
            var productRepository = new ProductsController(_httpClient, dbContext);
            ProductSoftDeleteVM product = new ProductSoftDeleteVM
            {
                Id = new Guid("51b489dd-cbe6-4328-8deb-08dd27926710")
            };

            var result = await productRepository.DeleteProduct(product);

            result.Should().NotBeNull();
            result.Should().BeOfType<OkObjectResult>();
        }
        [Fact]
        public async void ProductController_DeleteProduct_ReturnsBadRequest()
        {
            var dbContext = await GetDatabaseContext();
            var productRepository = new ProductsController(_httpClient, dbContext);
            ProductSoftDeleteVM product = null;

            var result = await productRepository.DeleteProduct(product);

            result.Should().NotBeNull();
            result.Should().BeOfType<BadRequestObjectResult>();
        }
        [Fact]
        public async void ProductController_GetCheaperProductsThan_ReturnsOk()
        {
            var dbContext = await GetDatabaseContext();
            var productRepository = new ProductsController(_httpClient, dbContext);
            decimal value = 100;

            var result = await productRepository.GetCheaperProductsThan(value);

            result.Should().NotBeNull();
            result.Should().BeOfType<OkObjectResult>();
        }
        [Fact]
        public async void ProductController_GetCheaperProductsThan_ReturnsBadRequest()
        {
            var dbContext = await GetDatabaseContext();
            var productRepository = new ProductsController(_httpClient, dbContext);
            decimal value = 0;

            var result = await productRepository.GetCheaperProductsThan(value);

            result.Should().NotBeNull();
            result.Should().BeOfType<BadRequestObjectResult>();
        }
        [Fact]
        public async void ProductController_GetCheaperProductsThanIsAvilable_ReturnsOk()
        {
            var dbContext = await GetDatabaseContext();
            var productRepository = new ProductsController(_httpClient, dbContext);
            decimal value = 1000;
            bool isIt = true;

            var result = await productRepository.GetCheaperProductsThanIsAvilable(value, isIt);

            result.Should().NotBeNull();
            result.Should().BeOfType<OkObjectResult>();

        }
        [Fact]
        public async void ProductController_GetCheaperProductsThanIsAvilable_ReturnsBadRequest()
        {
            var dbContext = await GetDatabaseContext();
            var productRepository = new ProductsController(_httpClient, dbContext);
            decimal value = 10;
            bool isIt = true;

            var result = await productRepository.GetCheaperProductsThanIsAvilable(value, isIt);

            result.Should().NotBeNull();
            result.Should().BeOfType<BadRequestObjectResult>();

        }
    }
}
