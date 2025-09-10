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
    }
}