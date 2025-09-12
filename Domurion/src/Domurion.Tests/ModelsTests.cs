using Domurion.Models;

namespace Domurion.Tests
{
    public class ModelsTests
    {
        [Fact]
        public void User_Defaults_AreSet()
        {
            var user = new User();
            Assert.NotEqual(Guid.Empty, user.Id);
            Assert.Equal(string.Empty, user.Username);
            Assert.Equal(string.Empty, user.PasswordHash);
        }

        [Fact]
        public void Credential_Defaults_AreSet()
        {
            var cred = new Credential();
            Assert.NotEqual(Guid.Empty, cred.Id);
            Assert.Equal(string.Empty, cred.Site);
            Assert.Equal(string.Empty, cred.Username);
            Assert.Equal(string.Empty, cred.EncryptedPassword);
            Assert.Equal(string.Empty, cred.IntegrityHash);
            Assert.Equal(Guid.Empty, cred.UserId);
        }

        [Fact]
        public void AuditLog_Defaults_AreSet()
        {
            var log = new AuditLog();
            Assert.NotEqual(Guid.Empty, log.Id);
            Assert.Equal(Guid.Empty, log.UserId);
            Assert.Equal(string.Empty, log.Username);
            Assert.Null(log.CredentialId);
            Assert.Equal(string.Empty, log.Action);
            Assert.True((DateTime.UtcNow - log.Timestamp).TotalSeconds < 5);
            Assert.Equal(string.Empty, log.IpAddress);
            Assert.Null(log.Site);
        }

        [Fact]
        public void PasswordHistory_Defaults_AreSet()
        {
            var ph = new PasswordHistory();
            Assert.NotEqual(Guid.Empty, ph.Id);
            Assert.Equal(Guid.Empty, ph.UserId);
            Assert.Equal(string.Empty, ph.PasswordHash);
            Assert.True((DateTime.UtcNow - ph.ChangedAt).TotalSeconds < 5);
        }

        [Fact]
        public void User_RequiredFields_Enforced()
        {
            var user = new User();
            Assert.Throws<ArgumentNullException>(() => user.Username = null!);
            Assert.Throws<ArgumentNullException>(() => user.PasswordHash = null!);
        }

        [Fact]
        public void Credential_RequiredFields_Enforced()
        {
            var cred = new Credential();
            Assert.Throws<ArgumentNullException>(() => cred.Site = null!);
            Assert.Throws<ArgumentNullException>(() => cred.Username = null!);
            Assert.Throws<ArgumentNullException>(() => cred.EncryptedPassword = null!);
            Assert.Throws<ArgumentNullException>(() => cred.IntegrityHash = null!);
        }
    }
}
