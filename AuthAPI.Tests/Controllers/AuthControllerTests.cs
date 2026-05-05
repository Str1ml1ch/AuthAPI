using AuthAPI.Controllers;
using AuthAPI.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;

namespace AuthAPI.Tests.Controllers;

public class AuthControllerTests
{
    private static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    private static IConfiguration CreateConfig() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"]            = "EventsBookingsSuperSecretKey1234567890",
                ["Jwt:Issuer"]         = "AuthAPI",
                ["Jwt:Audience"]       = "EventsBookingApps",
                ["Jwt:ExpiryMinutes"]  = "60"
            })
            .Build();


    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenUserNotFound()
    {
        var userManager = CreateUserManagerMock();
        userManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                   .ReturnsAsync((ApplicationUser?)null);
        var controller = new AuthController(userManager.Object, CreateConfig());

        var result = await controller.Login(new LoginRequest { Email = "x@x.com", Password = "pass" });

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenPasswordInvalid()
    {
        var userManager = CreateUserManagerMock();
        var appUser = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = "u@u.com", User = new AuthAPI.DAL.Entities.User { Id = Guid.NewGuid(), FirstName = "A", LastName = "B", CreatedAt = DateTimeOffset.UtcNow, ApplicationUserId = Guid.NewGuid().ToString() } };
        userManager.Setup(m => m.FindByEmailAsync("u@u.com")).ReturnsAsync(appUser);
        userManager.Setup(m => m.CheckPasswordAsync(appUser, "wrongpass")).ReturnsAsync(false);
        var controller = new AuthController(userManager.Object, CreateConfig());

        var result = await controller.Login(new LoginRequest { Email = "u@u.com", Password = "wrongpass" });

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task Login_ReturnsOkWithToken_WhenCredentialsValid()
    {
        var userManager = CreateUserManagerMock();
        var appUser = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = "valid@u.com",
            User = new AuthAPI.DAL.Entities.User
            {
                Id = Guid.NewGuid(),
                FirstName = "Valid",
                LastName = "User",
                CreatedAt = DateTimeOffset.UtcNow,
                ApplicationUserId = Guid.NewGuid().ToString()
            }
        };
        userManager.Setup(m => m.FindByEmailAsync("valid@u.com")).ReturnsAsync(appUser);
        userManager.Setup(m => m.CheckPasswordAsync(appUser, "correctpass")).ReturnsAsync(true);
        userManager.Setup(m => m.GetRolesAsync(appUser)).ReturnsAsync(new List<string> { "User" });
        var controller = new AuthController(userManager.Object, CreateConfig());

        var result = await controller.Login(new LoginRequest { Email = "valid@u.com", Password = "correctpass" });

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
        var tokenProp = ok.Value!.GetType().GetProperty("token");
        Assert.NotNull(tokenProp);
        var token = tokenProp.GetValue(ok.Value) as string;
        Assert.False(string.IsNullOrWhiteSpace(token));
    }


    [Fact]
    public async Task Register_ReturnsBadRequest_WhenCreateFails()
    {
        var userManager = CreateUserManagerMock();
        userManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                   .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password too short" }));
        var controller = new AuthController(userManager.Object, CreateConfig());
        var request = new RegisterRequest { Email = "new@u.com", Password = "weak", FirstName = "New", LastName = "User" };

        var result = await controller.Register(request);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Register_ReturnsOk_WhenCreateSucceeds()
    {
        var userManager = CreateUserManagerMock();
        userManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                   .ReturnsAsync(IdentityResult.Success);
        var controller = new AuthController(userManager.Object, CreateConfig());
        var request = new RegisterRequest { Email = "new@u.com", Password = "Str0ng!", FirstName = "New", LastName = "User" };

        var result = await controller.Register(request);

        Assert.IsType<OkResult>(result);
    }
}
