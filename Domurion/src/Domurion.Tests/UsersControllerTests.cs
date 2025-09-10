using Domurion.Controllers;
using Domurion.Dtos;
using Domurion.Services;
using Domurion.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Domurion.Tests
{
    public class UsersControllerTests
    {
        private static Data.DataContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<Data.DataContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new Data.DataContext(options);
        }

        private static UsersController CreateController(Data.DataContext context)
        {
            IUserService userService = new UserService(context);
            return new UsersController(userService);
        }

        #region Register
        [Fact]
        public void Register_Success_ReturnsOk()
        {
            var context = CreateInMemoryContext();
            var controller = CreateController(context);
            var dto = new UserDto { Username = "newuser", Password = "StrongP@ssw0rd!" };
            var result = controller.Register(dto);
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);
            var username = ok.Value.GetType().GetProperty("Username")?.GetValue(ok.Value, null) as string;
            Assert.Equal("newuser", username);
        }

        [Fact]
        public void Register_DuplicateUser_ReturnsConflict()
        {
            var context = CreateInMemoryContext();
            var controller = CreateController(context);
            var dto = new UserDto { Username = "dupuser", Password = "StrongP@ssw0rd!" };
            controller.Register(dto);
            var result = controller.Register(dto);
            var conflict = Assert.IsType<ConflictObjectResult>(result);
            Assert.NotNull(conflict.Value);
            Assert.Contains("already exists", conflict.Value.ToString());
        }

        [Fact]
        public void Register_InvalidInput_ReturnsBadRequest()
        {
            var context = CreateInMemoryContext();
            var controller = CreateController(context);
            var dto = new UserDto { Username = "", Password = "" };
            var result = controller.Register(dto);
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequest.Value);
            Assert.Contains("required", badRequest.Value.ToString());
        }
        #endregion

        #region Login
        [Fact]
        public void Login_Success_ReturnsOk()
        {
            var context = CreateInMemoryContext();
            var controller = CreateController(context);
            var dto = new UserDto { Username = "loginuser", Password = "StrongP@ssw0rd!" };
            controller.Register(dto);
            var result = controller.Login(dto);
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);
            var username = ok.Value.GetType().GetProperty("Username")?.GetValue(ok.Value, null) as string;
            Assert.Equal("loginuser", username);
        }

        [Fact]
        public void Login_WrongPassword_ReturnsUnauthorized()
        {
            var context = CreateInMemoryContext();
            var controller = CreateController(context);
            var dto = new UserDto { Username = "loginuser2", Password = "StrongP@ssw0rd!" };
            controller.Register(dto);
            var wrongDto = new UserDto { Username = "loginuser2", Password = "WrongPassword1!" };
            var result = controller.Login(wrongDto);
            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.NotNull(unauthorized.Value);
            Assert.Contains("Invalid username or password", unauthorized.Value.ToString());
        }

        [Fact]
        public void Login_NonExistentUser_ReturnsUnauthorized()
        {
            var context = CreateInMemoryContext();
            var controller = CreateController(context);
            var dto = new UserDto { Username = "nouser", Password = "StrongP@ssw0rd!" };
            var result = controller.Login(dto);
            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.NotNull(unauthorized.Value);
            Assert.Contains("Invalid username or password", unauthorized.Value.ToString());
        }
        #endregion
    }
}