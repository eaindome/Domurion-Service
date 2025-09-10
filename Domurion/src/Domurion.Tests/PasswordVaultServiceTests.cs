using Domurion.Services;
using Microsoft.EntityFrameworkCore;

namespace Domurion.Tests
{
    public class PasswordVaultServiceTests
    {
        private static void SetTestEnvironmentVariables()
        {
            Environment.SetEnvironmentVariable("AES_KEY", Convert.ToBase64String(new byte[32]));
            Environment.SetEnvironmentVariable("AES_IV", Convert.ToBase64String(new byte[16]));
            Environment.SetEnvironmentVariable("HMAC_KEY", Convert.ToBase64String(new byte[32]));
        }
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
            SetTestEnvironmentVariables();
            var context = CreateInMemoryContext();
            var vaultService = new PasswordVaultService(context);
            var credential = vaultService.AddCredential(TestUserId, "example.com", "john", "StrongP@ssw0rd!");
            Assert.NotNull(credential);
            Assert.Equal("example.com", credential.Site);
            Assert.Equal("john", credential.Username);
            Assert.NotEqual("StrongP@ssw0rd!", credential.EncryptedPassword); // Ensure encrypted
        }

        [Fact]
        public void AddCredential_WeakPassword_ShouldThrow()
        {
            SetTestEnvironmentVariables();
            var context = CreateInMemoryContext();
            var vaultService = new PasswordVaultService(context);
            Assert.Throws<ArgumentException>(() => vaultService.AddCredential(TestUserId, "site.com", "user", "weak"));
        }

        [Fact]
        public void AddCredential_DuplicateSiteUsername_Allows()
        {
            SetTestEnvironmentVariables();
            var context = CreateInMemoryContext();
            var vaultService = new PasswordVaultService(context);
            var cred1 = vaultService.AddCredential(TestUserId, "site.com", "user", "StrongP@ssw0rd!");
            var cred2 = vaultService.AddCredential(TestUserId, "site.com", "user", "AnotherStr0ngP@ss!");
            Assert.NotEqual(cred1.Id, cred2.Id);
        }
        [Fact]
        public void AddCredential_AuditLog_IsWritten()
        {
            SetTestEnvironmentVariables();
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
            SetTestEnvironmentVariables();
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
            SetTestEnvironmentVariables();
            vaultService.AddCredential(userId1, "site1.com", "user1", "StrongP@ssw0rd!");
            vaultService.AddCredential(userId2, "site2.com", "user2", "NewStr0ngP@ss1!");
            var results = vaultService.GetCredentials(userId1);
            Assert.Single(results); // Only user1’s creds
            Assert.Equal("site1.com", results.First().Site);
        }
        #endregion

        #region RetrievePassword
        [Fact]
        public void RetrievePassword_AuditLog_IsWritten()
        {
            SetTestEnvironmentVariables();
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
            SetTestEnvironmentVariables();
            var credential = vaultService.AddCredential(userId, "mysite.com", "john", "StrongP@ssw0rd!");
            var password = vaultService.RetrievePassword(credential.Id, userId);
            Assert.Equal("StrongP@ssw0rd!", password);
        }

        [Fact]
        public void RetrievePassword_WithWrongUser_ShouldThrow()
        {
            var context = CreateInMemoryContext();
            var vaultService = new PasswordVaultService(context);
            var userId = TestUserId;
            SetTestEnvironmentVariables();
            var credential = vaultService.AddCredential(userId, "mysite.com", "john", "StrongP@ssw0rd!");
            Assert.Throws<KeyNotFoundException>(() => vaultService.RetrievePassword(credential.Id, TestUserId));
        }

        [Fact]
        public void RetrievePassword_WithWrongCredentialId_ShouldThrow()
        {
            var context = CreateInMemoryContext();
            var vaultService = new PasswordVaultService(context);
            var userId = TestUserId;
            SetTestEnvironmentVariables();
            vaultService.AddCredential(userId, "mysite.com", "john", "StrongP@ssw0rd!");
            Assert.Throws<KeyNotFoundException>(() => vaultService.RetrievePassword(Guid.NewGuid(), userId));
        }
        #endregion

        #region UpdateCredential
        [Fact]
        public void UpdateCredential_ChangeSiteAndUsername_Success()
        {
            SetTestEnvironmentVariables();
            var context = CreateInMemoryContext();
            var vaultService = new PasswordVaultService(context);
            var userId = TestUserId;
            var cred = vaultService.AddCredential(userId, "oldsite.com", "olduser", "StrongP@ssw0rd!");
            var updated = vaultService.UpdateCredential(cred.Id, userId, "newsite.com", "newuser", null);
            Assert.Equal("newsite.com", updated.Site);
            Assert.Equal("newuser", updated.Username);
        }

        [Fact]
        public void UpdateCredential_ChangePassword_Success()
        {
            SetTestEnvironmentVariables();
            var context = CreateInMemoryContext();
            var vaultService = new PasswordVaultService(context);
            var userId = TestUserId;
            var cred = vaultService.AddCredential(userId, "site.com", "user", "StrongP@ssw0rd!");
            var originalEncrypted = cred.EncryptedPassword;
            var updated = vaultService.UpdateCredential(cred.Id, userId, null, null, "NewStr0ngP@ss!");
            Assert.NotEqual(originalEncrypted, updated.EncryptedPassword);
        }

        [Fact]
        public void UpdateCredential_WeakPassword_Throws()
        {
            SetTestEnvironmentVariables();
            var context = CreateInMemoryContext();
            var vaultService = new PasswordVaultService(context);
            var userId = TestUserId;
            var cred = vaultService.AddCredential(userId, "site.com", "user", "StrongP@ssw0rd!");
            Assert.Throws<ArgumentException>(() => vaultService.UpdateCredential(cred.Id, userId, null, null, "weak"));
        }

        [Fact]
        public void UpdateCredential_NotFound_Throws()
        {
            SetTestEnvironmentVariables();
            var context = CreateInMemoryContext();
            var vaultService = new PasswordVaultService(context);
            var userId = TestUserId;
            Assert.Throws<KeyNotFoundException>(() => vaultService.UpdateCredential(Guid.NewGuid(), userId, "site", "user", "StrongP@ssw0rd!"));
        }

        [Fact]
        public void UpdateCredential_AuditLog_IsWritten()
        {
            SetTestEnvironmentVariables();
            var context = CreateInMemoryContext();
            var vaultService = new PasswordVaultService(context);
            var userId = TestUserId;
            var cred = vaultService.AddCredential(userId, "site.com", "user", "StrongP@ssw0rd!");
            var updated = vaultService.UpdateCredential(cred.Id, userId, "newsite.com", null, null);
            var log = context.AuditLogs.FirstOrDefault(l => l.UserId == userId && l.CredentialId == cred.Id && l.Action == "UpdateCredential");
            Assert.NotNull(log);
            Assert.Equal("newsite.com", log.Site);
        }
        #endregion
        
        #region DeleteCredential
        [Fact]
        public void DeleteCredential_RemovesCredential()
        {
            SetTestEnvironmentVariables();
            var context = CreateInMemoryContext();
            var vaultService = new PasswordVaultService(context);
            var userId = TestUserId;
            var cred = vaultService.AddCredential(userId, "site.com", "user", "StrongP@ssw0rd!");
            vaultService.DeleteCredential(cred.Id, userId);
            Assert.Empty(context.Credentials.Where(c => c.Id == cred.Id));
        }

        [Fact]
        public void DeleteCredential_NotFound_Throws()
        {
            SetTestEnvironmentVariables();
            var context = CreateInMemoryContext();
            var vaultService = new PasswordVaultService(context);
            var userId = TestUserId;
            Assert.Throws<KeyNotFoundException>(() => vaultService.DeleteCredential(Guid.NewGuid(), userId));
        }

        [Fact]
        public void DeleteCredential_AuditLog_IsWritten()
        {
            SetTestEnvironmentVariables();
            var context = CreateInMemoryContext();
            var vaultService = new PasswordVaultService(context);
            var userId = TestUserId;
            var cred = vaultService.AddCredential(userId, "site.com", "user", "StrongP@ssw0rd!");
            vaultService.DeleteCredential(cred.Id, userId);
            var log = context.AuditLogs.FirstOrDefault(l => l.UserId == userId && l.CredentialId == cred.Id && l.Action == "DeleteCredential");
            Assert.NotNull(log);
            Assert.Equal("site.com", log.Site);
        }
        #endregion
    }
}