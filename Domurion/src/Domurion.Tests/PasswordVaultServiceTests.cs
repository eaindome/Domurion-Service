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

        #region ShareCredential
        [Fact]
        public void ShareCredential_Success_DuplicatesCredentialForRecipient()
        {
            SetTestEnvironmentVariables();
            var context = CreateInMemoryContext();
            // Add two users
            var user1 = context.Users.Add(new Models.User { Username = "alice", PasswordHash = "hash" }).Entity;
            var user2 = context.Users.Add(new Models.User { Username = "bob", PasswordHash = "hash" }).Entity;
            context.SaveChanges();
            var vaultService = new PasswordVaultService(context);
            var cred = vaultService.AddCredential(user1.Id, "site.com", "user", "StrongP@ssw0rd!");
            var shared = vaultService.ShareCredential(cred.Id, user1.Id, "bob");
            Assert.NotEqual(cred.Id, shared.Id);
            Assert.Equal(user2.Id, shared.UserId);
            Assert.Equal(cred.Site, shared.Site);
            Assert.Equal(cred.Username, shared.Username);
        }

        [Fact]
        public void ShareCredential_SelfShare_Throws()
        {
            SetTestEnvironmentVariables();
            var context = CreateInMemoryContext();
            var user = context.Users.Add(new Models.User { Username = "alice", PasswordHash = "hash" }).Entity;
            context.SaveChanges();
            var vaultService = new PasswordVaultService(context);
            var cred = vaultService.AddCredential(user.Id, "site.com", "user", "StrongP@ssw0rd!");
            Assert.Throws<ArgumentException>(() => vaultService.ShareCredential(cred.Id, user.Id, "alice"));
        }

        [Fact]
        public void ShareCredential_RecipientNotFound_Throws()
        {
            SetTestEnvironmentVariables();
            var context = CreateInMemoryContext();
            var user = context.Users.Add(new Models.User { Username = "alice", PasswordHash = "hash" }).Entity;
            context.SaveChanges();
            var vaultService = new PasswordVaultService(context);
            var cred = vaultService.AddCredential(user.Id, "site.com", "user", "StrongP@ssw0rd!");
            Assert.Throws<KeyNotFoundException>(() => vaultService.ShareCredential(cred.Id, user.Id, "bob"));
        }

        [Fact]
        public void ShareCredential_CredentialNotFound_Throws()
        {
            SetTestEnvironmentVariables();
            var context = CreateInMemoryContext();
            var user1 = context.Users.Add(new Models.User { Username = "alice", PasswordHash = "hash" }).Entity;
            var user2 = context.Users.Add(new Models.User { Username = "bob", PasswordHash = "hash" }).Entity;
            context.SaveChanges();
            var vaultService = new PasswordVaultService(context);
            Assert.Throws<KeyNotFoundException>(() => vaultService.ShareCredential(Guid.NewGuid(), user1.Id, "bob"));
        }

        [Fact]
        public void ShareCredential_AuditLogs_AreWritten()
        {
            SetTestEnvironmentVariables();
            var context = CreateInMemoryContext();
            var user1 = context.Users.Add(new Models.User { Username = "alice", PasswordHash = "hash" }).Entity;
            var user2 = context.Users.Add(new Models.User { Username = "bob", PasswordHash = "hash" }).Entity;
            context.SaveChanges();
            var vaultService = new PasswordVaultService(context);
            var cred = vaultService.AddCredential(user1.Id, "site.com", "user", "StrongP@ssw0rd!");
            var shared = vaultService.ShareCredential(cred.Id, user1.Id, "bob");
            var log1 = context.AuditLogs.FirstOrDefault(l => l.UserId == user1.Id && l.CredentialId == cred.Id && l.Action == "ShareCredential");
            var log2 = context.AuditLogs.FirstOrDefault(l => l.UserId == user2.Id && l.CredentialId == shared.Id && l.Action == "ReceiveSharedCredential");
            Assert.NotNull(log1);
            Assert.NotNull(log2);
            Assert.Equal("site.com", log1.Site);
            Assert.Equal("site.com", log2.Site);
        }
        #endregion

        #region DataIntegrity
        [Fact]
        public void RetrievePassword_DataIntegrityFailure_Throws()
        {
            SetTestEnvironmentVariables();
            var context = CreateInMemoryContext();
            var vaultService = new PasswordVaultService(context);
            var userId = TestUserId;
            var cred = vaultService.AddCredential(userId, "site.com", "user", "StrongP@ssw0rd!");
            // Tamper with integrity hash
            cred.IntegrityHash = "invalidhash";
            context.SaveChanges();
            var ex = Assert.Throws<InvalidOperationException>(() => vaultService.RetrievePassword(cred.Id, userId));
            Assert.Contains("integrity", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void RetrievePassword_DataIntegrityFailure_DoesNotWriteAuditLog()
        {
            SetTestEnvironmentVariables();
            var context = CreateInMemoryContext();
            var vaultService = new PasswordVaultService(context);
            var userId = TestUserId;
            var cred = vaultService.AddCredential(userId, "site.com", "user", "StrongP@ssw0rd!");
            cred.IntegrityHash = "invalidhash";
            context.SaveChanges();
            try
            {
                _ = vaultService.RetrievePassword(cred.Id, userId);
            }
            catch (InvalidOperationException) { }
            // Should not log on failure
            var log = context.AuditLogs.FirstOrDefault(l => l.UserId == userId && l.CredentialId == cred.Id && l.Action == "RetrievePassword");
            Assert.Null(log);
        }
        #endregion

        #region ImportExport
        [Fact]
        public void ExportCredentials_ReturnsAllWithPasswords()
        {
            SetTestEnvironmentVariables();
            var context = CreateInMemoryContext();
            var vaultService = new PasswordVaultService(context);
            var userId = TestUserId;
            var cred1 = vaultService.AddCredential(userId, "site1.com", "user1", "StrongP@ssw0rd!");
            var cred2 = vaultService.AddCredential(userId, "site2.com", "user2", "NewStr0ngP@ss1!");
            var credentials = vaultService.GetCredentials(userId).ToList();
            var exported = credentials.Select(c => new {
                c.Site,
                c.Username,
                Password = vaultService.RetrievePassword(c.Id, userId)
            }).ToList();
            Assert.Equal(2, exported.Count);
            Assert.Contains(exported, e => e.Site == "site1.com" && e.Password == "StrongP@ssw0rd!");
            Assert.Contains(exported, e => e.Site == "site2.com" && e.Password == "NewStr0ngP@ss1!");
        }

        [Fact]
        public void ImportCredentials_SuccessAndPartialFailure()
        {
            SetTestEnvironmentVariables();
            var context = CreateInMemoryContext();
            var vaultService = new PasswordVaultService(context);
            var userId = TestUserId;
            var importList = new[] {
                new { Site = "site1.com", Username = "user1", Password = "StrongP@ssw0rd!" },
                new { Site = "", Username = "", Password = "" }
            };
            var imported = new List<object>();
            foreach (var cred in importList)
            {
                try
                {
                    var c = vaultService.AddCredential(userId, cred.Site, cred.Username, cred.Password);
                    imported.Add(new { c.Id, c.Site, c.Username });
                }
                catch (Exception ex)
                {
                    imported.Add(new { cred.Site, cred.Username, error = ex.Message });
                }
            }
            Assert.Equal(2, imported.Count);
            Assert.NotNull(imported[0].GetType().GetProperty("Id")?.GetValue(imported[0], null));
            var error = imported[1].GetType().GetProperty("error")?.GetValue(imported[1], null) as string;
            Assert.False(string.IsNullOrWhiteSpace(error));
        }
        #endregion
    }
}