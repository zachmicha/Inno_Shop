using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ProductManagement.Data;
using ProductManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagement.Controllers;
using UserManagement.Data;
using UserManagement.Models;
using UserManagement.ViewModels;

namespace Inno_Shop.Tests.Controllers
{
    public class UserControllerTest
    {
        
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        public UserControllerTest(HttpClient httpClient,IConfiguration configuration)
        {            
            _configuration = A.Fake<IConfiguration>();
            A.CallTo(() => _configuration["JwtSettings"]).Returns("MyFakeConfigValue");
            _httpClient = A.Fake<HttpClient>();
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
                        IsDeleted=false,
                        UserName=$"NewUser{i}",
                        Email=$"newEmail{i}",
                        EmailConfirmed=true,
                        PasswordHash= new PasswordHasher<User>().HashPassword(null, "P@ssword123!"),
                        ConcurrencyStamp = "17b4ecfb-4a75-4103-956d-dc43b5b27873"
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

        //[Fact]
        //public async Task UserController_Login_ReturnsOk()
        //{
        //    //Arrange
        //    var userManager = await GetUserManagerAsync();
        //    var userRepository = new UserController(userManager, _configuration, _httpClient);
        //    UserLoginVM loginVM = new UserLoginVM
        //    {
        //        Email="newEmail0",
        //        Password="P@ssword123!"
        //    };
        //    //Act
        //    var result = await userRepository.Login(loginVM);
        //    //Assert

        //    result.Should().NotBeNull();
        //    result.Should().BeOfType(typeof(OkObjectResult));
        //}
        [Fact]
        public async void UserController_GetUser_ReturnsOk()
        {

            var userManager = await GetUserManagerAsync();
                var userRepository = new UserController(userManager, _configuration, _httpClient);
                string id = "23ba54c2-e930-4189-9775-1b52b3a0ae70";

                var result = userRepository.GetUser(id);

                result.Should().NotBeNull();
                result.Should().BeOfType(typeof(OkObjectResult));
          
        }
    }
}
