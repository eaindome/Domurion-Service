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
            var user = service.Register("testuser", "password123");
            Assert.NotNull(user);
            Assert.Equal("testuser", user.Username);
        }

        [Fact]
        public void Register_WithWhitespaceUsernameOrPassword_ShouldThrow()
        {
            var context = CreateInMemoryContext();
            var service = new Services.UserService(context);
            Assert.Throws<ArgumentException>(() => service.Register("   ", "password123"));
            Assert.Throws<ArgumentException>(() => service.Register("testuser", "   "));
        }

        [Fact]
        public void Register_WithLongUsernameOrPassword_ShouldCreateUser()
        {
            var context = CreateInMemoryContext();
            var service = new Services.UserService(context);
            string longUsername = new string('u', 256);
            string longPassword = new string('p', 256);
            var user = service.Register(longUsername, longPassword);
            Assert.NotNull(user);
            Assert.Equal(longUsername, user.Username);
        }

        [Fact]
        public void Register_WithExistingUsername_ShouldThrow()
        {
            var context = CreateInMemoryContext();
            var service = new Services.UserService(context);
            service.Register("testuser", "password123");
            Assert.Throws<InvalidOperationException>(() => service.Register("testuser", "password456"));
        }

        [Fact]
        public void Register_WithInvalidData_ShouldThrow()
        {
            var context = CreateInMemoryContext();
            var service = new Services.UserService(context);
            Assert.Throws<ArgumentException>(() => service.Register("", "password123"));
        }

        [Fact]
        public void Register_SamePassword_DifferentHash()
        {
            var context = CreateInMemoryContext();
            var service = new Services.UserService(context);
            var user1 = service.Register("userA", "samepassword");
            var user2 = service.Register("userB", "samepassword");
            Assert.NotEqual(user1.PasswordHash, user2.PasswordHash);
        }
        #endregion

        #region Login Tests
        [Fact]
        public void Login_WithCorrectCredentials_ShouldReturnUser()
        {
            var context = CreateInMemoryContext();
            var service = new Services.UserService(context);
            service.Register("testuser", "password123");
            var user = service.Login("testuser", "password123");
            Assert.NotNull(user);
            Assert.Equal("testuser", user.Username);
        }

        [Fact]
        public void Login_WithNonExistentUsername_ShouldReturnNull()
        {
            var context = CreateInMemoryContext();
            var service = new Services.UserService(context);
            var user = service.Login("nouser", "password123");
            Assert.Null(user);
        }

        [Fact]
        public void Login_AfterMultipleRegistrations_ShouldReturnCorrectUser()
        {
            var context = CreateInMemoryContext();
            var service = new Services.UserService(context);
            service.Register("user1", "pass1");
            service.Register("user2", "pass2");
            var user1 = service.Login("user1", "pass1");
            var user2 = service.Login("user2", "pass2");
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
            service.Register("testuser", "password123");
            var user = service.Login("testuser", "wrongpassword");
            Assert.Null(user);
        }
        #endregion
    }
}