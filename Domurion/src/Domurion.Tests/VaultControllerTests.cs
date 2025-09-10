using Domurion.Controllers;
using Domurion.Services;
using Domurion.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace Domurion.Tests
{
    public class VaultControllerTests
    {
        #region Helper
        private static Data.DataContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<Data.DataContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new Data.DataContext(options);
        }

        private static VaultController CreateController(Data.DataContext context)
        {
            IPasswordVaultService vaultService = new PasswordVaultService(context);
            return new VaultController(vaultService);
        }
        private static void SetTestEnvironmentVariables()
        {
            Environment.SetEnvironmentVariable("AES_KEY", Convert.ToBase64String(new byte[32]));
            Environment.SetEnvironmentVariable("AES_IV", Convert.ToBase64String(new byte[16]));
            Environment.SetEnvironmentVariable("HMAC_KEY", Convert.ToBase64String(new byte[32]));
        }
        #endregion

        #region Add
        [Fact]
        public void Add_Success_ReturnsOk()
        {
            SetTestEnvironmentVariables();
            var context = CreateInMemoryContext();
            var controller = CreateController(context);
            // Setup mock HttpContext with dummy RemoteIpAddress
            var httpContext = new DefaultHttpContext();
            httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Loopback;
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            var userId = Guid.NewGuid();
            var result = controller.Add(userId, "example.com", "john", "StrongP@ssw0rd!");
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);
            var site = ok.Value.GetType().GetProperty("Site")?.GetValue(ok.Value, null) as string;
            Assert.Equal("example.com", site);
        }

        [Fact]
        public void Add_InvalidInput_ReturnsBadRequest()
        {
            SetTestEnvironmentVariables();
            var context = CreateInMemoryContext();
            var controller = CreateController(context);
            // Setup mock HttpContext with dummy RemoteIpAddress
            var httpContext = new DefaultHttpContext();
            httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Loopback;
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            var userId = Guid.NewGuid();
            var result = controller.Add(userId, "", "", "");
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequest.Value);
            Assert.Contains("required", badRequest.Value.ToString());
        }
        #endregion

        
    }
}
