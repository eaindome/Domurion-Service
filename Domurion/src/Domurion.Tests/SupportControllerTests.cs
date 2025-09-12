using Domurion.Controllers;
using Domurion.Data;
using Domurion.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Domurion.Tests
{
    public class SupportControllerTests
    {
        private static DataContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new DataContext(options);
        }

        [Fact]
        public void Request2FAReset_Success_ReturnsOk()
        {
            var context = CreateInMemoryContext();
            var controller = new SupportController(context);
            var req = new SupportRequest { Username = "alice" };
            var result = controller.Request2FAReset(req);
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("submitted", ok.Value!.ToString(), System.StringComparison.OrdinalIgnoreCase);
            Assert.Single(context.SupportRequests);
        }

        [Fact]
        public void Request2FAReset_Validation_ReturnsBadRequest()
        {
            var context = CreateInMemoryContext();
            var controller = new SupportController(context);
            var req = new SupportRequest();
            var result = controller.Request2FAReset(req);
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("required", badRequest.Value!.ToString(), System.StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void GetRequests_ReturnsUnresolvedRequests()
        {
            var context = CreateInMemoryContext();
            context.SupportRequests.Add(new SupportRequest { Id = Guid.NewGuid(), Username = "bob", RequestedAt = DateTime.UtcNow, Resolved = false });
            context.SupportRequests.Add(new SupportRequest { Id = Guid.NewGuid(), Username = "eve", RequestedAt = DateTime.UtcNow, Resolved = true });
            context.SaveChanges();
            var controller = new SupportController(context);
            var result = controller.GetRequests();
            var ok = Assert.IsType<OkObjectResult>(result);
            var list = Assert.IsAssignableFrom<System.Collections.IEnumerable>(ok.Value);
            Assert.Single(list.Cast<object>());
        }

        [Fact]
        public void Resolve2FAReset_Success_Resets2FAAndResolves()
        {
            var context = CreateInMemoryContext();
            var user = context.Users.Add(new User { Username = "alice", PasswordHash = "hash", TwoFactorEnabled = true, TwoFactorSecret = "secret", TwoFactorRecoveryCodes = "codes" }).Entity;
            var req = context.SupportRequests.Add(new SupportRequest { Id = Guid.NewGuid(), Username = "alice", RequestedAt = DateTime.UtcNow, Resolved = false }).Entity;
            context.SaveChanges();
            var controller = new SupportController(context);
            var result = controller.Resolve2FAReset(req.Id, "reset by admin");
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("reset", ok.Value!.ToString(), System.StringComparison.OrdinalIgnoreCase);
            context.Entry(user).Reload();
            Assert.False(user.TwoFactorEnabled);
            Assert.Null(user.TwoFactorSecret);
            Assert.Null(user.TwoFactorRecoveryCodes);
            context.Entry(req).Reload();
            Assert.True(req.Resolved);
            Assert.NotNull(req.ResolvedAt);
            Assert.Equal("reset by admin", req.ResolutionNote);
        }

        [Fact]
        public void Resolve2FAReset_RequestNotFound_ReturnsNotFound()
        {
            var context = CreateInMemoryContext();
            var controller = new SupportController(context);
            var result = controller.Resolve2FAReset(Guid.NewGuid(), "note");
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("not found", notFound.Value!.ToString(), System.StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Resolve2FAReset_UserNotFound_ReturnsNotFound()
        {
            var context = CreateInMemoryContext();
            var req = context.SupportRequests.Add(new SupportRequest { Id = Guid.NewGuid(), Username = "ghost", RequestedAt = DateTime.UtcNow, Resolved = false }).Entity;
            context.SaveChanges();
            var controller = new SupportController(context);
            var result = controller.Resolve2FAReset(req.Id, "note");
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("User not found", notFound.Value!.ToString(), System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
