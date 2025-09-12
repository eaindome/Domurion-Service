using Domurion.Helpers;
using Xunit;

namespace Domurion.Tests
{
    public class HelpersTests
    {
        #region CryptoHelper
        [Fact]
        public void EncryptPassword_And_DecryptPassword_Roundtrip()
        {
            // Set env vars for AES
            Environment.SetEnvironmentVariable("AES_KEY", Convert.ToBase64String(new byte[32]));
            Environment.SetEnvironmentVariable("AES_IV", Convert.ToBase64String(new byte[16]));
            var password = "StrongP@ssw0rd!";
            var encrypted = CryptoHelper.EncryptPassword(password);
            Assert.NotEqual(password, encrypted);
            var decrypted = CryptoHelper.DecryptPassword(encrypted);
            Assert.Equal(password, decrypted);
        }

        [Fact]
        public void EncryptPassword_ThrowsIfKeyMissing()
        {
            Environment.SetEnvironmentVariable("AES_KEY", null);
            Environment.SetEnvironmentVariable("AES_IV", Convert.ToBase64String(new byte[16]));
            Assert.Throws<InvalidOperationException>(() => CryptoHelper.EncryptPassword("test"));
        }

        [Fact]
        public void EncryptPassword_ThrowsIfIVMissing()
        {
            Environment.SetEnvironmentVariable("AES_KEY", Convert.ToBase64String(new byte[32]));
            Environment.SetEnvironmentVariable("AES_IV", null);
            Assert.Throws<InvalidOperationException>(() => CryptoHelper.EncryptPassword("test"));
        }

        [Fact]
        public void EncryptPassword_ThrowsIfKeyWrongLength()
        {
            Environment.SetEnvironmentVariable("AES_KEY", Convert.ToBase64String(new byte[16]));
            Environment.SetEnvironmentVariable("AES_IV", Convert.ToBase64String(new byte[16]));
            Assert.Throws<InvalidOperationException>(() => CryptoHelper.EncryptPassword("test"));
        }

        [Fact]
        public void EncryptPassword_ThrowsIfIVWrongLength()
        {
            Environment.SetEnvironmentVariable("AES_KEY", Convert.ToBase64String(new byte[32]));
            Environment.SetEnvironmentVariable("AES_IV", Convert.ToBase64String(new byte[8]));
            Assert.Throws<InvalidOperationException>(() => CryptoHelper.EncryptPassword("test"));
        }
        #endregion
        #region Test
        [Theory]
        [InlineData("StrongP@ssw0rd!", true)]
        [InlineData("weak", false)]
        [InlineData("NoSpecial123", false)]
        [InlineData("NoDigit!@#", false)]
        [InlineData("noupper1!", false)]
        [InlineData("NOLOWER1!", false)]
        public void IsStrongPassword_Works(string password, bool expected)
        {
            Assert.Equal(expected, Helper.IsStrongPassword(password));
        }

        [Fact]
        public void HashPassword_And_VerifyPassword_Work()
        {
            var password = "StrongP@ssw0rd!";
            var hash = Helper.HashPassword(password);
            Assert.True(Helper.VerifyPassword(password, hash));
            Assert.False(Helper.VerifyPassword("WrongPassword", hash));
        }

        [Fact]
        public void ComputeHmac_ProducesConsistentResult()
        {
            var data = "sensitive-data";
            var key = Convert.ToBase64String(new byte[32]);
            var hmac1 = Helper.ComputeHmac(data, key);
            var hmac2 = Helper.ComputeHmac(data, key);
            Assert.Equal(hmac1, hmac2);
        }

        [Fact]
        public void GeneratePassword_ProducesStrongPassword()
        {
            var password = Helper.GeneratePassword(16);
            Assert.True(Helper.IsStrongPassword(password));
            Assert.Equal(16, password.Length);
        }

        [Fact]
        public void GeneratePassword_EnforcesMinimumLength()
        {
            var password = Helper.GeneratePassword(4);
            Assert.True(password.Length >= 8);
        }
        #endregion
    }
}
