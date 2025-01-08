using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using ProductManagement.API.Controllers;
using ProductManagement.Application.Interfaces;
using ProductManagement.Application.ViewModels;
using ProductManagement.Domain.Entities;
using ProductManagement.Infrastructure.Databases;

namespace Inno_Shop.Tests.Controllers
{
    public class ProductsControllerTest
    {
        private readonly IProductService _productService;
        public ProductsControllerTest()
        {
            _productService= A.Fake<IProductService>();
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
            A.CallTo(() => _productService.GetAllProductsAsync()).Returns(dbContext.Products.ToListAsync());
            var productsController = new ProductsController(_productService);
            //Act
            var result = await productsController.GetAllProducts();

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType(typeof(OkObjectResult));
           
        }

        [Fact]
        public async void ProductController_GetAllAvilableProductsAssignedTOUserId_ReturnsOk()
        {
            var dbContext = await GetDatabaseContext();
            A.CallTo(() => _productService.GetAvailableProductsByUserIdAsync(new Guid("23ba54c2-e930-4189-9775-1b52b3a0ae79"), true)).Returns(dbContext.Products.ToListAsync());
            var productsController = new ProductsController(_productService);

            //Act
            var result = await productsController.GetAllAvilableProductsAssignedToUserId(new Guid("23ba54c2-e930-4189-9775-1b52b3a0ae79"),true);

            result.Should().NotBeNull();            
            result.Should().BeOfType(typeof(OkObjectResult));           
        }
        [Fact]
        public async void ProductController_GetAllAvilableProductsAssignedTOUserId_ReturnsBadRequest()
        {
            var userId = Guid.Empty;
            var dbContext = await GetDatabaseContext();
            A.CallTo(() => _productService.GetAvailableProductsByUserIdAsync(userId, true)).Returns(dbContext.Products.ToListAsync());
            var productsController = new ProductsController(_productService);

            //Act
            var result = await productsController.GetAllAvilableProductsAssignedToUserId(userId, false);

            result.Should().NotBeNull();
            result.Should().BeOfType(typeof(BadRequestObjectResult));
        }

        [Fact]
        public async void ProductController_ChangeAvailabilityOfProduct_ReturnsOk()
        {
            //Arrange
            var dbContext = await GetDatabaseContext();
            var productId = new Guid("51b489dd-cbe6-4328-8deb-08dd27926710");
            var isAvailable = false;
            A.CallTo(() => _productService.ChangeAvailabilityAsync(productId, isAvailable))
                  .ReturnsLazily(async () =>
                  {
                      var product = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == productId);
                      if (product == null)
                          return false;

                      product.IsAvailable = isAvailable;
                      await dbContext.SaveChangesAsync();
                      return true;
                  });
            var productController = new ProductsController(_productService);
            //Act
            var result = await productController.ChangeAvailabilityOfProduct(isAvailable, productId);
                
            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType(typeof(OkObjectResult));

        }
        [Fact]
        public async void ProductController_ChangeAvailabilityOfProduct_ReturnsBadRequest()
        {
            //Arrange
            var dbContext = await GetDatabaseContext();
            var productId = Guid.Empty;
            var isAvailable = false;
            A.CallTo(() => _productService.ChangeAvailabilityAsync(productId, isAvailable))
                  .ReturnsLazily(async () =>
                  {
                      var product = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == productId);
                      if (product == null)
                          return false;

                      product.IsAvailable = isAvailable;
                      await dbContext.SaveChangesAsync();
                      return true;
                  });
            var productController = new ProductsController(_productService);
            //Act
            var result = await productController.ChangeAvailabilityOfProduct(isAvailable, productId);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType(typeof(BadRequestObjectResult));

        }
        [Fact]
        public async void ProductController_CreateProduct_ReturnsOk()
        {
            //Arrange
            var dbContext = await GetDatabaseContext();
            var productRepository = new ProductsController(_productService);
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
            var productRepository = new ProductsController(_productService);
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
            var productId = new Guid("51b489dd-cbe6-4328-8deb-08dd27926710");
            var productDto = new ProductSoftDeleteVM { Id = productId };

            A.CallTo(() => _productService.DeleteProductAsync(productId))
       .ReturnsLazily(async () =>
       {
           var product = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == productId);
           if (product == null || product.IsDeleted)
               return false;

           product.IsDeleted = true;
           await dbContext.SaveChangesAsync();
           return true;
       });
            var productController = new ProductsController(_productService);

            // Act
            var result = await productController.DeleteProduct(productDto);

            result.Should().NotBeNull();
            result.Should().BeOfType<OkObjectResult>();

            A.CallTo(() => _productService.DeleteProductAsync(productId))
        .MustHaveHappenedOnceExactly();
        }
        [Fact]
        public async void ProductController_DeleteProduct_ReturnsBadRequest()
        {
            var dbContext = await GetDatabaseContext();
            var productId = Guid.Empty;
            var productDto = new ProductSoftDeleteVM { Id = productId };

            A.CallTo(() => _productService.DeleteProductAsync(productId))
       .ReturnsLazily(async () =>
       {
           var product = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == productId);
           if (product == null || product.IsDeleted)
               return false;

           product.IsDeleted = true;
           await dbContext.SaveChangesAsync();
           return true;
       });
            var productController = new ProductsController(_productService);

            // Act
            var result = await productController.DeleteProduct(productDto);

            result.Should().NotBeNull();
            result.Should().BeOfType<BadRequestObjectResult>();
        }
        [Fact]
        public async void ProductController_GetCheaperProductsThan_ReturnsOk()
        {
            var dbContext = await GetDatabaseContext();
            decimal price = 100;

            A.CallTo(() => _productService.GetProductsCheaperThanAsync(price))
       .ReturnsLazily(async () =>
       {
           var products = await dbContext.Products
               .Where(p => p.Price < price && !p.IsDeleted)
               .ToListAsync();

           return products;
       });

            var productController = new ProductsController(_productService);

            // Act
            var result = await productController.GetCheaperProductsThan(price);

            result.Should().NotBeNull();
            result.Should().BeOfType<OkObjectResult>();

            A.CallTo(() => _productService.GetProductsCheaperThanAsync(price))
      .MustHaveHappenedOnceExactly();
        }
        [Fact]
        public async void ProductController_GetCheaperProductsThan_ReturnsBadRequest()
        {
            var dbContext = await GetDatabaseContext();
            decimal price = 0;

            A.CallTo(() => _productService.GetProductsCheaperThanAsync(price))
       .ReturnsLazily(async () =>
       {
           var products = await dbContext.Products
               .Where(p => p.Price < price && !p.IsDeleted)
               .ToListAsync();

           return products;
       });

            var productController = new ProductsController(_productService);

            // Act
            var result = await productController.GetCheaperProductsThan(price);

            result.Should().NotBeNull();
            result.Should().BeOfType<NotFoundObjectResult>();

            A.CallTo(() => _productService.GetProductsCheaperThanAsync(price))
      .MustHaveHappenedOnceExactly();
        }
        [Fact]
        public async void ProductController_GetCheaperProductsThanIsAvilable_ReturnsOk()
        {
            var dbContext = await GetDatabaseContext();
            decimal price = 100;
            bool isAvilable = true;
            A.CallTo(() => _productService.GetProductsCheaperThanAndAvailableAsync(price,isAvilable))
       .ReturnsLazily(async () =>
       {
           var products = await dbContext.Products
               .Where(p => p.Price < price && !p.IsDeleted && p.IsAvailable==isAvilable)
               .ToListAsync();

           return products;
       });

            var productController = new ProductsController(_productService);

            // Act
            var result = await productController.GetCheaperProductsThanIsAvilable(price,isAvilable);

            result.Should().NotBeNull();
            result.Should().BeOfType<OkObjectResult>();

            A.CallTo(() => _productService.GetProductsCheaperThanAndAvailableAsync(price,isAvilable))
      .MustHaveHappenedOnceExactly();

        }
        [Fact]
        public async void ProductController_GetCheaperProductsThanIsAvilable_ReturnsBadRequest()
        {
            var dbContext = await GetDatabaseContext();
            decimal price = 10;
            bool isAvilable = true;
            A.CallTo(() => _productService.GetProductsCheaperThanAndAvailableAsync(price, isAvilable))
       .ReturnsLazily(async () =>
       {
           var products = await dbContext.Products
               .Where(p => p.Price < price && !p.IsDeleted && p.IsAvailable == isAvilable)
               .ToListAsync();

           return products;
       });

            var productController = new ProductsController(_productService);

            // Act
            var result = await productController.GetCheaperProductsThanIsAvilable(price, isAvilable);

            result.Should().NotBeNull();
            result.Should().BeOfType<NotFoundObjectResult>();

            A.CallTo(() => _productService.GetProductsCheaperThanAndAvailableAsync(price, isAvilable))
      .MustHaveHappenedOnceExactly();
        }
    }
}
