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

        #region Update
        [Fact]
        public void Update_Success_ChangesUsername()
        {
            var context = CreateInMemoryContext();
            var controller = CreateController(context);
            var regResult = controller.Register(new UserDto { Username = "olduser", Password = "StrongP@ssw0rd!" });
            var user = Assert.IsType<OkObjectResult>(regResult);
            Assert.NotNull(user.Value);
            var idProp = user.Value?.GetType().GetProperty("Id");
            Assert.NotNull(idProp);
            var idObj = idProp.GetValue(user.Value, null);
            Assert.NotNull(idObj);
            var userId = (Guid)idObj;
            var result = controller.Update(userId, "newuser", null);
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);
            var usernameProp = ok.Value.GetType().GetProperty("Username");
            Assert.NotNull(usernameProp);
            var usernameObj = usernameProp.GetValue(ok.Value, null);
            Assert.NotNull(usernameObj);
            Assert.Equal("newuser", usernameObj);
        }

        [Fact]
        public void Update_DuplicateUsername_ReturnsConflict()
        {
            var context = CreateInMemoryContext();
            var controller = CreateController(context);
            var regResult1 = controller.Register(new UserDto { Username = "user1", Password = "StrongP@ssw0rd!" });
            var regResult2 = controller.Register(new UserDto { Username = "user2", Password = "StrongP@ssw0rd!" });
            var user2 = Assert.IsType<OkObjectResult>(regResult2);
            Assert.NotNull(user2.Value);
            var idProp = user2.Value?.GetType().GetProperty("Id");
            Assert.NotNull(idProp);
            var idObj = idProp.GetValue(user2.Value, null);
            Assert.NotNull(idObj);
            var user2Id = (Guid)idObj;
            var result = controller.Update(user2Id, "user1", null);
            var conflict = Assert.IsType<ConflictObjectResult>(result);
            Assert.NotNull(conflict.Value);
            Assert.Contains("already exists", conflict.Value.ToString());
        }

        [Fact]
        public void Update_WeakPassword_ReturnsBadRequest()
        {
            var context = CreateInMemoryContext();
            var controller = CreateController(context);
            var regResult = controller.Register(new UserDto { Username = "user", Password = "StrongP@ssw0rd!" });
            var user = Assert.IsType<OkObjectResult>(regResult);
            Assert.NotNull(user.Value);
            var idProp = user.Value?.GetType().GetProperty("Id");
            Assert.NotNull(idProp);
            var idObj = idProp.GetValue(user.Value, null);
            Assert.NotNull(idObj);
            var userId = (Guid)idObj;
            var result = controller.Update(userId, null, "weak");
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequest.Value);
            Assert.Contains("strength", badRequest.Value.ToString());
        }

        [Fact]
        public void Update_PasswordReuse_ReturnsBadRequest()
        {
            var context = CreateInMemoryContext();
            var controller = CreateController(context);
            var regResult = controller.Register(new UserDto { Username = "user", Password = "StrongP@ssw0rd!" });
            var user = Assert.IsType<OkObjectResult>(regResult);
            Assert.NotNull(user.Value);
            var idProp = user.Value?.GetType().GetProperty("Id");
            Assert.NotNull(idProp);
            var idObj = idProp.GetValue(user.Value, null);
            Assert.NotNull(idObj);
            var userId = (Guid)idObj;
            // Change password 5 times
            for (int i = 0; i < 5; i++)
            {
                controller.Update(userId, null, $"NewStr0ngP@ss{i}!");
            }
            // Try to reuse original password
            var result = controller.Update(userId, null, "StrongP@ssw0rd!");
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequest.Value);
            Assert.Contains("reuse", badRequest.Value.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Update_UserNotFound_ReturnsNotFound()
        {
            var context = CreateInMemoryContext();
            var controller = CreateController(context);
            var missingId = Guid.NewGuid();
            var result = controller.Update(missingId, "newuser", "StrongP@ssw0rd!");
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFound.Value);
            Assert.Contains("not found", notFound.Value.ToString(), StringComparison.OrdinalIgnoreCase);
        }
        #endregion

        #region Delete
        [Fact]
        public void Delete_Success_ReturnsNoContent()
        {
            var context = CreateInMemoryContext();
            var controller = CreateController(context);
            var regResult = controller.Register(new UserDto { Username = "todelete", Password = "StrongP@ssw0rd!" });
            var user = Assert.IsType<OkObjectResult>(regResult);
            Assert.NotNull(user.Value);
            var idProp = user.Value?.GetType().GetProperty("Id");
            Assert.NotNull(idProp);
            var idObj = idProp.GetValue(user.Value, null);
            Assert.NotNull(idObj);
            var userId = (Guid)idObj;
            var result = controller.Delete(userId);
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public void Delete_UserNotFound_ReturnsNotFound()
        {
            var context = CreateInMemoryContext();
            var controller = CreateController(context);
            var missingId = Guid.NewGuid();
            var result = controller.Delete(missingId);
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFound.Value);
            Assert.Contains("not found", notFound.Value.ToString(), StringComparison.OrdinalIgnoreCase);
        }
        #endregion
    }
}