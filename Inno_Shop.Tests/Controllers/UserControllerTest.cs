using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using UserManagement.API.Controllers;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Services;
using UserManagement.Application.ViewModels;
using UserManagement.Domain.Entities;
using UserManagement.Infrastructure.Databases;

namespace Inno_Shop.Tests.Controllers
{
    public class UserControllerTest
    {

        private readonly IConfiguration _configuration;
        // private readonly HttpClient _httpClient;
        private readonly IUserService _userService;
        public UserControllerTest()
        {
           // _httpClient = A.Fake<HttpClient>();
           _userService = A.Fake<IUserService>();
            var inMemorySettings = new Dictionary<string, string>
{
    { "JwtSettings:Issuer", "https://myapi.example.com" },
    { "JwtSettings:Audience", "https://myapi.example.com" },
    { "JwtSettings:Key", "MIHcAgEBBEIBXkN7oKY8LbHtRDSjysc3BdF" },
    { "JwtSettings:Time", "60" }
};


            _configuration = new ConfigurationBuilder()
    .AddInMemoryCollection(inMemorySettings)
    .Build();

        }
        private async Task<UserManager<User>> GetUserManagerAsync()
        {
            var options = new DbContextOptionsBuilder<UserDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var databaseContext = new UserDbContext(options);
            databaseContext.Database.EnsureCreated();
            if (await databaseContext.Users.CountAsync() <= 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    databaseContext.Users.Add(new User
                    {
                        Id = $"23ba54c2-e930-4189-9775-1b52b3a0ae7{i}",
                        IsDeleted = false,
                        UserName = $"NewUser{i}",
                        Email = $"newEmail{i}@.com",
                        EmailConfirmed = true,
                        PasswordHash = new PasswordHasher<User>().HashPassword(null, "P@ssword123!")
                    });
                }
                await databaseContext.SaveChangesAsync();
            }
            var userStore = new UserStore<User>(databaseContext);
            var userManager = new UserManager<User>(
                userStore,
                null, // OptionsAccessor (not needed for in-memory)
                new PasswordHasher<User>(),
                new IUserValidator<User>[0],
                new IPasswordValidator<User>[0],
                new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(),
                null, // Services
                null  // Logger
            );

            return userManager;
        }
        [Fact]
        public async void UserController_GetUser_ReturnsOk()
        {
            //Arrange
            var userManager = await GetUserManagerAsync(); // Creates UserManager with in-memory DB
            var userService = new UserService(userManager, _configuration); // Real instance of UserService
            var userController = new UserController(userService); // Pass real UserService to controller
            string userId = "23ba54c2-e930-4189-9775-1b52b3a0ae70";
            //Act
            var result = await userController.GetUser(userId);
           //Assert

            result.Should().NotBeNull();
            result.Should().BeOfType<OkObjectResult>();

            var okResult = result as OkObjectResult;
            var user = okResult?.Value as UserReadOnlyVM;
            user.Should().NotBeNull();
            user?.Id.Should().Be(userId);
        }

        [Fact]
        public async void UserController_UpdateEmailUser_ReturnsOk()
        {
            //Arrange
            var userManager = await GetUserManagerAsync(); // Creates UserManager with in-memory DB
            var userService = new UserService(userManager, _configuration); // Real instance of UserService
            var userController = new UserController(userService); // Pass real UserService to controller
            string userId = "23ba54c2-e930-4189-9775-1b52b3a0ae70";
            UserUpdateEmailVM userDto = new UserUpdateEmailVM
            {
                Email = "damn@gmail.com"
            };
            //Act
            var result = await userController.UpdateEmailUser(userId, userDto);
            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType(typeof(OkObjectResult));


        }
        [Fact]
        public async void UserController_UpdateEmailAndPasswordUser_ReturnsOk()
        {
            //Arrange
            var userManager = await GetUserManagerAsync(); // Creates UserManager with in-memory DB
            var userService = new UserService(userManager, _configuration); // Real instance of UserService
            var userController = new UserController(userService); // Pass real UserService to controller
            string userId = "23ba54c2-e930-4189-9775-1b52b3a0ae70";
            UserUpdateEmailAndPasswordVM userDto = new UserUpdateEmailAndPasswordVM
            {
                Email = "damn@gmail.com",
                CurrentPassword = "P@ssword123!",
                Password = "P@ssword321!"
            };
            //Act
            var result = await userController.UpdateEmailAndPasswordUser(userId, userDto);
            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType(typeof(OkObjectResult));

        }

        [Fact]
        public async void UserController_UpdatePasswordUser_ReturnsOk()
        {
            //Arrange
            var userManager = await GetUserManagerAsync(); // Creates UserManager with in-memory DB
            var userService = new UserService(userManager, _configuration); // Real instance of UserService
            var userController = new UserController(userService); // Pass real UserService to controller
            string userId = "23ba54c2-e930-4189-9775-1b52b3a0ae70";
            UserUpdatePasswordVM userDto = new UserUpdatePasswordVM
            {
                CurrentPassword = "P@ssword123!",
                Password = "damn@gmail.com1132"
            };
            //Act
            var result = await userController.UpdatePasswordUser(userId, userDto);
            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType(typeof(OkObjectResult));

        }
        [Fact]
        public async void UserController_DeleteUser_ReturnsOk()
        {
            //Arrange
            var userManager = await GetUserManagerAsync(); // Creates UserManager with in-memory DB
            var userService = new UserService(userManager, _configuration); // Real instance of UserService
            var userController = new UserController(userService); // Pass real UserService to controller
            string userId = "23ba54c2-e930-4189-9775-1b52b3a0ae70";

            //Act
            var result = await userController.DeleteUser(userId);
            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType(typeof(OkObjectResult));

        }

    }
}
