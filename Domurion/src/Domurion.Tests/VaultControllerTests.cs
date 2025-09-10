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

        #region List
        [Fact]
        public void List_WithCredentials_ReturnsOnlyForUser()
        {
            SetTestEnvironmentVariables();
            var context = CreateInMemoryContext();
            var controller = CreateController(context);
            var httpContext = new DefaultHttpContext();
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            var userId1 = Guid.NewGuid();
            var userId2 = Guid.NewGuid();
            // Add credentials for both users
            var vaultService = new PasswordVaultService(context);
            vaultService.AddCredential(userId1, "site1.com", "user1", "StrongP@ssw0rd!");
            vaultService.AddCredential(userId2, "site2.com", "user2", "StrongP@ssw0rd!");
            var result = controller.List(userId1);
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);
            var creds = ok.Value as IEnumerable<object>;
            Assert.NotNull(creds);
            Assert.Single(creds);
            var site = creds.First().GetType().GetProperty("Site")?.GetValue(creds.First(), null) as string;
            Assert.Equal("site1.com", site);
        }

        [Fact]
        public void List_Empty_ReturnsEmptyList()
        {
            SetTestEnvironmentVariables();
            var context = CreateInMemoryContext();
            var controller = CreateController(context);
            var httpContext = new DefaultHttpContext();
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            var userId = Guid.NewGuid();
            var result = controller.List(userId);
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);
            var creds = ok.Value as IEnumerable<object>;
            Assert.NotNull(creds);
            Assert.Empty(creds);
        }
        #endregion

        #region Retrieve
        [Fact]
        public void Retrieve_Success_ReturnsPassword()
        {
            SetTestEnvironmentVariables();
            var context = CreateInMemoryContext();
            var controller = CreateController(context);
            var httpContext = new DefaultHttpContext();
            httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Loopback;
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            var userId = Guid.NewGuid();
            var vaultService = new PasswordVaultService(context);
            var cred = vaultService.AddCredential(userId, "site.com", "user", "StrongP@ssw0rd!");
            var result = controller.Retrieve(cred.Id, userId);
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);
            var password = ok.Value.GetType().GetProperty("Password")?.GetValue(ok.Value, null) as string;
            Assert.Equal("StrongP@ssw0rd!", password);
        }

        [Fact]
        public void Retrieve_WrongUser_ReturnsNotFound()
        {
            SetTestEnvironmentVariables();
            var context = CreateInMemoryContext();
            var controller = CreateController(context);
            var httpContext = new DefaultHttpContext();
            httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Loopback;
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var vaultService = new PasswordVaultService(context);
            var cred = vaultService.AddCredential(userId, "site.com", "user", "StrongP@ssw0rd!");
            var result = controller.Retrieve(cred.Id, otherUserId);
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFound.Value);
            Assert.Contains("not found", notFound.Value.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Retrieve_NotFound_ReturnsNotFound()
        {
            SetTestEnvironmentVariables();
            var context = CreateInMemoryContext();
            var controller = CreateController(context);
            var httpContext = new DefaultHttpContext();
            httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Loopback;
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            var userId = Guid.NewGuid();
            var missingCredId = Guid.NewGuid();
            var result = controller.Retrieve(missingCredId, userId);
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFound.Value);
            Assert.Contains("not found", notFound.Value.ToString(), StringComparison.OrdinalIgnoreCase);
        }
        #endregion       

        #region Export
        [Fact]
        public void Export_ReturnsAllCredentialsWithPasswords()
        {
            SetTestEnvironmentVariables();
            var context = CreateInMemoryContext();
            var controller = CreateController(context);
            var userId = Guid.NewGuid();
            var vaultService = new PasswordVaultService(context);
            vaultService.AddCredential(userId, "site1.com", "user1", "StrongP@ssw0rd!");
            vaultService.AddCredential(userId, "site2.com", "user2", "NewStr0ngP@ss1!");
            var httpContext = new DefaultHttpContext();
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            var result = controller.Export(userId);
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);
            var creds = (ok.Value as IEnumerable<object>)?.ToList();
            Assert.NotNull(creds);
            Assert.Equal(2, creds.Count);
            var passwords = creds.Select(c => c.GetType().GetProperty("Password")?.GetValue(c, null) as string).ToList();
            Assert.Contains("StrongP@ssw0rd!", passwords);
            Assert.Contains("NewStr0ngP@ss1!", passwords);
        }
        #endregion

        #region Import
        [Fact]
        public void Import_Success_ReturnsImported()
        {
            SetTestEnvironmentVariables();
            var context = CreateInMemoryContext();
            var controller = CreateController(context);
            var userId = Guid.NewGuid();
            var httpContext = new DefaultHttpContext();
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            var importList = new List<Domurion.Dtos.ImportCredentialDto> {
                new() { Site = "site1.com", Username = "user1", Password = "StrongP@ssw0rd!" },
                new() { Site = "site2.com", Username = "user2", Password = "NewStr0ngP@ss1!" }
            };
            var result = controller.Import(userId, importList);
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);
            var imported = (ok.Value as IEnumerable<object>)?.ToList();
            Assert.NotNull(imported);
            Assert.Equal(2, imported.Count);
            Assert.All(imported, i => Assert.NotNull(i.GetType().GetProperty("Id")?.GetValue(i, null)));
        }

        [Fact]
        public void Import_PartialFailure_ReturnsErrors()
        {
            SetTestEnvironmentVariables();
            var context = CreateInMemoryContext();
            var controller = CreateController(context);
            var userId = Guid.NewGuid();
            var httpContext = new DefaultHttpContext();
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            var importList = new List<Domurion.Dtos.ImportCredentialDto> {
                new() { Site = "site1.com", Username = "user1", Password = "StrongP@ssw0rd!" },
                new() { Site = "", Username = "", Password = "" }
            };
            var result = controller.Import(userId, importList);
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);
            var imported = (ok.Value as IEnumerable<object>)?.ToList();
            Assert.NotNull(imported);
            Assert.Equal(2, imported.Count);
            Assert.NotNull(imported[0].GetType().GetProperty("Id")?.GetValue(imported[0], null));
            var error = imported[1].GetType().GetProperty("error")?.GetValue(imported[1], null) as string;
            Assert.False(string.IsNullOrWhiteSpace(error));
        }
        #endregion

        #region Share
        [Fact]
        public void Share_Success_ReturnsOk()
        {
            SetTestEnvironmentVariables();
            var context = CreateInMemoryContext();
            var controller = CreateController(context);
            var httpContext = new DefaultHttpContext();
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            // Add two users
            var user1 = context.Users.Add(new Domurion.Models.User { Username = "alice", PasswordHash = "hash" }).Entity;
            var user2 = context.Users.Add(new Domurion.Models.User { Username = "bob", PasswordHash = "hash" }).Entity;
            context.SaveChanges();
            var vaultService = new PasswordVaultService(context);
            var cred = vaultService.AddCredential(user1.Id, "site.com", "user", "StrongP@ssw0rd!");
            var result = controller.Share(cred.Id, user1.Id, "bob");
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);
            var sharedSite = ok.Value.GetType().GetProperty("Site")?.GetValue(ok.Value, null) as string;
            Assert.Equal("site.com", sharedSite);
        }

        [Fact]
        public void Share_SelfShare_ReturnsBadRequest()
        {
            SetTestEnvironmentVariables();
            var context = CreateInMemoryContext();
            var controller = CreateController(context);
            var httpContext = new DefaultHttpContext();
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            var user = context.Users.Add(new Domurion.Models.User { Username = "alice", PasswordHash = "hash" }).Entity;
            context.SaveChanges();
            var vaultService = new PasswordVaultService(context);
            var cred = vaultService.AddCredential(user.Id, "site.com", "user", "StrongP@ssw0rd!");
            var result = controller.Share(cred.Id, user.Id, "alice");
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequest.Value);
            Assert.Contains("yourself", badRequest.Value.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Share_RecipientNotFound_ReturnsNotFound()
        {
            SetTestEnvironmentVariables();
            var context = CreateInMemoryContext();
            var controller = CreateController(context);
            var httpContext = new DefaultHttpContext();
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            var user = context.Users.Add(new Domurion.Models.User { Username = "alice", PasswordHash = "hash" }).Entity;
            context.SaveChanges();
            var vaultService = new PasswordVaultService(context);
            var cred = vaultService.AddCredential(user.Id, "site.com", "user", "StrongP@ssw0rd!");
            var result = controller.Share(cred.Id, user.Id, "bob");
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFound.Value);
            Assert.Contains("recipient", notFound.Value.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Share_CredentialNotFound_ReturnsNotFound()
        {
            SetTestEnvironmentVariables();
            var context = CreateInMemoryContext();
            var controller = CreateController(context);
            var httpContext = new DefaultHttpContext();
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            var user1 = context.Users.Add(new Domurion.Models.User { Username = "alice", PasswordHash = "hash" }).Entity;
            var user2 = context.Users.Add(new Domurion.Models.User { Username = "bob", PasswordHash = "hash" }).Entity;
            context.SaveChanges();
            var result = controller.Share(Guid.NewGuid(), user1.Id, "bob");
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFound.Value);
            Assert.Contains("not found", notFound.Value.ToString(), StringComparison.OrdinalIgnoreCase);
        }
        #endregion 
    }
}
