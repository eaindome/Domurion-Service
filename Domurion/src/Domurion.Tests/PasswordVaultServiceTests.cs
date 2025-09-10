using Domurion.Services;
using Microsoft.EntityFrameworkCore;

namespace Domurion.Tests
{
    public class PasswordVaultServiceTests
    {
        static PasswordVaultServiceTests()
        {
            DotNetEnv.Env.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".env"));
        }
        private static Guid TestUserId => Guid.NewGuid();
        private static Data.DataContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<Data.DataContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new Data.DataContext(options);
        }

        #region AddCredential
        [Fact]
        public void AddCredential_ShouldStoreCredential()
        {
            var context = CreateInMemoryContext();
            var vaultService = new PasswordVaultService(context);
            var credential = vaultService.AddCredential(TestUserId, "example.com", "john", "secret123");
            Assert.NotNull(credential);
            Assert.Equal("example.com", credential.Site);
            Assert.Equal("john", credential.Username);
            Assert.NotEqual("secret123", credential.EncryptedPassword); // Ensure encrypted
        }

        [Fact]
        public void AddCredential_WeakPassword_ShouldThrow()
        {
            var context = CreateInMemoryContext();
            var vaultService = new PasswordVaultService(context);
            Assert.Throws<ArgumentException>(() => vaultService.AddCredential(TestUserId, "site.com", "user", "weak"));
        }

        [Fact]
        public void AddCredential_DuplicateSiteUsername_Allows()
        {
            var context = CreateInMemoryContext();
            var vaultService = new PasswordVaultService(context);
            var cred1 = vaultService.AddCredential(TestUserId, "site.com", "user", "StrongP@ssw0rd!");
            var cred2 = vaultService.AddCredential(TestUserId, "site.com", "user", "AnotherStr0ngP@ss!");
            Assert.NotEqual(cred1.Id, cred2.Id);
        }
        [Fact]
        public void AddCredential_AuditLog_IsWritten()
        {
            var context = CreateInMemoryContext();
            var vaultService = new PasswordVaultService(context);
            var userId = TestUserId;
            var credential = vaultService.AddCredential(userId, "site.com", "user", "StrongP@ssw0rd!");
            var log = context.AuditLogs.FirstOrDefault(l => l.UserId == userId && l.CredentialId == credential.Id && l.Action == "AddCredential");
            Assert.NotNull(log);
            Assert.Equal("site.com", log.Site);
        }

        [Fact]
        public void AddCredential_WithInvalidData_ShouldThrow()
        {
            var context = CreateInMemoryContext();
            var vaultService = new PasswordVaultService(context);
            Assert.Throws<ArgumentException>(() => vaultService.AddCredential(TestUserId, "", "user", "pass"));
            Assert.Throws<ArgumentException>(() => vaultService.AddCredential(TestUserId, "site", "", "pass"));
            Assert.Throws<ArgumentException>(() => vaultService.AddCredential(TestUserId, "site", "user", ""));
        }
        #endregion

        #region GetCredentials
        [Fact]
        public void GetCredentials_ShouldReturnForCorrectUser()
        {
            var context = CreateInMemoryContext();
            var vaultService = new PasswordVaultService(context);
            var userId1 = TestUserId;
            var userId2 = TestUserId;
            vaultService.AddCredential(userId1, "site1.com", "user1", "pass1");
            vaultService.AddCredential(userId2, "site2.com", "user2", "pass2");
            var results = vaultService.GetCredentials(userId1);
            Assert.Single(results); // Only user1’s creds
            Assert.Equal("site1.com", results.First().Site);
        }
        #endregion

        #region RetrievePassword
        [Fact]
        public void RetrievePassword_AuditLog_IsWritten()
        {
            var context = CreateInMemoryContext();
            var vaultService = new PasswordVaultService(context);
            var userId = TestUserId;
            var credential = vaultService.AddCredential(userId, "site.com", "user", "StrongP@ssw0rd!");
            var _ = vaultService.RetrievePassword(credential.Id, userId);
            var log = context.AuditLogs.FirstOrDefault(l => l.UserId == userId && l.CredentialId == credential.Id && l.Action == "RetrievePassword");
            Assert.NotNull(log);
            Assert.Equal("site.com", log.Site);
        }
        [Fact]
        public void RetrievePassword_ShouldReturnOriginalPassword()
        {
            var context = CreateInMemoryContext();
            var vaultService = new PasswordVaultService(context);
            var userId = TestUserId;
            var credential = vaultService.AddCredential(userId, "mysite.com", "john", "mypassword");
            var password = vaultService.RetrievePassword(credential.Id, userId);
            Assert.Equal("mypassword", password);
        }

        [Fact]
        public void RetrievePassword_WithWrongUser_ShouldThrow()
        {
            var context = CreateInMemoryContext();
            var vaultService = new PasswordVaultService(context);
            var userId = TestUserId;
            var credential = vaultService.AddCredential(userId, "mysite.com", "john", "mypassword");
            Assert.Throws<KeyNotFoundException>(() => vaultService.RetrievePassword(credential.Id, TestUserId));
        }

        [Fact]
        public void RetrievePassword_WithWrongCredentialId_ShouldThrow()
        {
            var context = CreateInMemoryContext();
            var vaultService = new PasswordVaultService(context);
            var userId = TestUserId;
            vaultService.AddCredential(userId, "mysite.com", "john", "mypassword");
            Assert.Throws<KeyNotFoundException>(() => vaultService.RetrievePassword(Guid.NewGuid(), userId));
        }
        #endregion
    }
}