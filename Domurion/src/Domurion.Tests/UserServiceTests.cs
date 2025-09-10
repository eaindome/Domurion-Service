using Microsoft.EntityFrameworkCore;

namespace Domurion.Tests
{
    public class UserServiceTests
    {
        private static Data.DataContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<Data.DataContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new Data.DataContext(options);
        }

        #region Register
        [Fact]
        public void Register_ShouldCreateUser()
        {
            var context = CreateInMemoryContext();
            var service = new Services.UserService(context);
            var user = service.Register("testuser", "StrongP@ssw0rd!");
            Assert.NotNull(user);
            Assert.Equal("testuser", user.Username);
        }

        [Fact]
        public void Register_WithWhitespaceUsernameOrPassword_ShouldThrow()
        {
            var context = CreateInMemoryContext();
            var service = new Services.UserService(context);
            Assert.Throws<ArgumentException>(() => service.Register("   ", "StrongP@ssw0rd!"));
            Assert.Throws<ArgumentException>(() => service.Register("testuser", "   "));
        }

        [Fact]
        public void Register_WithLongUsernameOrPassword_ShouldCreateUser()
        {
            var context = CreateInMemoryContext();
            var service = new Services.UserService(context);
            string longUsername = new string('u', 256);
            // Ensure long password meets policy: upper, lower, digit, special
            string longPassword = "Aa1!" + new string('p', 252);
            var user = service.Register(longUsername, longPassword);
            Assert.NotNull(user);
            Assert.Equal(longUsername, user.Username);
        }

        [Fact]
        public void Register_WithExistingUsername_ShouldThrow()
        {
            var context = CreateInMemoryContext();
            var service = new Services.UserService(context);
            service.Register("testuser", "StrongP@ssw0rd!");
            Assert.Throws<InvalidOperationException>(() => service.Register("testuser", "NewStr0ngP@ss1!"));
        }

        [Fact]
        public void Register_WithInvalidData_ShouldThrow()
        {
            var context = CreateInMemoryContext();
            var service = new Services.UserService(context);
            Assert.Throws<ArgumentException>(() => service.Register("", "StrongP@ssw0rd!"));
        }

        [Fact]
        public void Register_SamePassword_DifferentHash()
        {
            var context = CreateInMemoryContext();
            var service = new Services.UserService(context);
            var user1 = service.Register("userA", "StrongP@ssw0rd!");
            var user2 = service.Register("userB", "StrongP@ssw0rd!");
            Assert.NotEqual(user1.PasswordHash, user2.PasswordHash);
        }
        #endregion

        #region DeleteUser
        [Fact]
        public void DeleteUser_RemovesUser()
        {
            var context = CreateInMemoryContext();
            var service = new Services.UserService(context);
            var user = service.Register("todelete", "StrongP@ssw0rd!");
            service.DeleteUser(user.Id);
            Assert.Null(service.GetByUsername("todelete"));
        }

        [Fact]
        public void DeleteUser_RemovesCredentials()
        {
            var context = CreateInMemoryContext();
            var userService = new Services.UserService(context);
            var vaultService = new Services.PasswordVaultService(context);
            var user = userService.Register("todelete", "StrongP@ssw0rd!");
            // Set required env vars for vault service
            Environment.SetEnvironmentVariable("AES_KEY", Convert.ToBase64String(new byte[32]));
            Environment.SetEnvironmentVariable("AES_IV", Convert.ToBase64String(new byte[16]));
            Environment.SetEnvironmentVariable("HMAC_KEY", Convert.ToBase64String(new byte[32]));
            vaultService.AddCredential(user.Id, "site.com", "user", "StrongP@ssw0rd!");
            userService.DeleteUser(user.Id);
            Assert.Empty(context.Credentials.Where(c => c.UserId == user.Id));
        }

        [Fact]
        public void DeleteUser_UserNotFound_Throws()
        {
            var context = CreateInMemoryContext();
            var service = new Services.UserService(context);
            var missingId = Guid.NewGuid();
            Assert.Throws<KeyNotFoundException>(() => service.DeleteUser(missingId));
        }
        #endregion

        #region Login Tests
        [Fact]
        public void Login_WithCorrectCredentials_ShouldReturnUser()
        {
            var context = CreateInMemoryContext();
            var service = new Services.UserService(context);
            service.Register("testuser", "StrongP@ssw0rd!");
            var user = service.Login("testuser", "StrongP@ssw0rd!");
            Assert.NotNull(user);
            Assert.Equal("testuser", user.Username);
        }

        [Fact]
        public void Login_WithNonExistentUsername_ShouldReturnNull()
        {
            var context = CreateInMemoryContext();
            var service = new Services.UserService(context);
            var user = service.Login("nouser", "StrongP@ssw0rd!");
            Assert.Null(user);
        }

        [Fact]
        public void Login_AfterMultipleRegistrations_ShouldReturnCorrectUser()
        {
            var context = CreateInMemoryContext();
            var service = new Services.UserService(context);
            service.Register("user1", "StrongP@ssw0rd!");
            service.Register("user2", "NewStr0ngP@ss1!");
            var user1 = service.Login("user1", "StrongP@ssw0rd!");
            var user2 = service.Login("user2", "NewStr0ngP@ss1!");
            Assert.NotNull(user1);
            Assert.Equal("user1", user1.Username);
            Assert.NotNull(user2);
            Assert.Equal("user2", user2.Username);
        }

        [Fact]
        public void Login_WithIncorrectCredentials_ShouldReturnNull()
        {
            var context = CreateInMemoryContext();
            var service = new Services.UserService(context);
            service.Register("testuser", "StrongP@ssw0rd!");
            var user = service.Login("testuser", "WrongP@ssw0rd!");
            Assert.Null(user);
        }
        #endregion

        #region UpdateUser
        [Fact]
        public void UpdateUser_ChangeUsername_Success()
        {
            var context = CreateInMemoryContext();
            var service = new Services.UserService(context);
            var user = service.Register("olduser", "StrongP@ssw0rd!");
            var updated = service.UpdateUser(user.Id, "newuser", null);
            Assert.Equal("newuser", updated.Username);
        }

        [Fact]
        public void UpdateUser_ChangePassword_Success()
        {
            var context = CreateInMemoryContext();
            var service = new Services.UserService(context);
            var user = service.Register("user", "StrongP@ssw0rd!");
            var originalHash = user.PasswordHash;
            var updated = service.UpdateUser(user.Id, null, "NewStr0ngP@ss!");
            Assert.NotEqual(originalHash, updated.PasswordHash);
            Assert.True(Domurion.Helpers.Helper.VerifyPassword("NewStr0ngP@ss!", updated.PasswordHash));
        }

        [Fact]
        public void UpdateUser_DuplicateUsername_Throws()
        {
            var context = CreateInMemoryContext();
            var service = new Services.UserService(context);
            var user1 = service.Register("user1", "StrongP@ssw0rd!");
            var user2 = service.Register("user2", "StrongP@ssw0rd!");
            Assert.Throws<InvalidOperationException>(() => service.UpdateUser(user2.Id, "user1", null));
        }

        [Fact]
        public void UpdateUser_WeakPassword_Throws()
        {
            var context = CreateInMemoryContext();
            var service = new Services.UserService(context);
            var user = service.Register("user", "StrongP@ssw0rd!");
            Assert.Throws<ArgumentException>(() => service.UpdateUser(user.Id, null, "weak"));
        }

        [Fact]
        public void UpdateUser_PasswordReuse_Throws()
        {
            var context = CreateInMemoryContext();
            var service = new Services.UserService(context);
            var user = service.Register("user", "StrongP@ssw0rd!");
            // Change password 5 times
            for (int i = 0; i < 5; i++)
            {
                service.UpdateUser(user.Id, null, $"NewStr0ngP@ss{i}!");
            }
            // Try to reuse original password
            Assert.Throws<ArgumentException>(() => service.UpdateUser(user.Id, null, "StrongP@ssw0rd!"));
        }

        [Fact]
        public void UpdateUser_UserNotFound_Throws()
        {
            var context = CreateInMemoryContext();
            var service = new Services.UserService(context);
            var missingId = Guid.NewGuid();
            Assert.Throws<KeyNotFoundException>(() => service.UpdateUser(missingId, "newuser", "StrongP@ssw0rd!"));
        }
        #endregion
    }
}