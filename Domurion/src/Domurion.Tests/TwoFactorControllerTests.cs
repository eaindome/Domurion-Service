using Domurion.Controllers;
using Domurion.Models;
using Domurion.Services;
using Domurion.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Xunit;
using OtpNet;

namespace Domurion.Tests
{
    public class TwoFactorControllerTests
    {
        private static DataContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new DataContext(options);
        }

        private static TwoFactorController CreateController(DataContext context, Guid userId)
        {
            var userService = new UserService(context);
            var controller = new TwoFactorController(userService, context);
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
            return controller;
        }

        [Fact]
        public void Enable2FA_SetsSecretAndReturnsQRCode()
        {
            var context = CreateInMemoryContext();
            var user = context.Users.Add(new User { Username = "alice", PasswordHash = "hash" }).Entity;
            context.SaveChanges();
            var controller = CreateController(context, user.Id);
            var result = controller.Enable2FA();
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);
            var secret = ok.Value.GetType().GetProperty("secret")?.GetValue(ok.Value, null) as string;
            var qrCode = ok.Value.GetType().GetProperty("qrCode")?.GetValue(ok.Value, null) as string;
            Assert.False(string.IsNullOrWhiteSpace(secret));
            Assert.StartsWith("data:image/png;base64,", qrCode);
            context.Entry(user).Reload();
            Assert.False(string.IsNullOrWhiteSpace(user.TwoFactorSecret));
        }

        [Fact]
        public void Enable2FA_AlreadyEnabled_ReturnsBadRequest()
        {
            var context = CreateInMemoryContext();
            var user = context.Users.Add(new User { Username = "bob", PasswordHash = "hash", TwoFactorEnabled = true }).Entity;
            context.SaveChanges();
            var controller = CreateController(context, user.Id);
            var result = controller.Enable2FA();
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequest.Value);
            Assert.Contains("already enabled", badRequest.Value.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Verify2FA_Success_Enables2FA()
        {
            var context = CreateInMemoryContext();
            var user = context.Users.Add(new User { Username = "eve", PasswordHash = "hash" }).Entity;
            context.SaveChanges();
            var controller = CreateController(context, user.Id);
            // Enable 2FA to get secret
            var enableResult = controller.Enable2FA();
            var valueProp = enableResult.GetType().GetProperty("Value")?.GetValue(enableResult, null);
            var secretProp = valueProp?.GetType().GetProperty("secret")?.GetValue(valueProp, null) as string;
            Assert.False(string.IsNullOrWhiteSpace(secretProp));
            var secret = secretProp!;
            var totp = new Totp(Base32Encoding.ToBytes(secret));
            var code = totp.ComputeTotp();
            var verifyResult = controller.Verify2FA(code);
            var ok = Assert.IsType<OkObjectResult>(verifyResult);
            Assert.NotNull(ok.Value);
            Assert.Contains("enabled", ok.Value.ToString(), StringComparison.OrdinalIgnoreCase);
            context.Entry(user).Reload();
            Assert.True(user.TwoFactorEnabled);
        }

        [Fact]
        public void Verify2FA_InvalidCode_ReturnsBadRequest()
        {
            var context = CreateInMemoryContext();
            var user = context.Users.Add(new User { Username = "eve", PasswordHash = "hash" }).Entity;
            context.SaveChanges();
            var controller = CreateController(context, user.Id);
            controller.Enable2FA();
            var verifyResult = controller.Verify2FA("000000");
            var badRequest = Assert.IsType<BadRequestObjectResult>(verifyResult);
            Assert.NotNull(badRequest.Value);
            Assert.Contains("invalid", badRequest.Value.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Disable2FA_Success_Disables2FA()
        {
            var context = CreateInMemoryContext();
            var user = context.Users.Add(new User { Username = "dan", PasswordHash = "hash" }).Entity;
            context.SaveChanges();
            var controller = CreateController(context, user.Id);
            // Enable and verify 2FA
            var enableResult = controller.Enable2FA();
            var valueObj = enableResult.GetType().GetProperty("Value")?.GetValue(enableResult, null);
            var secretObj = valueObj?.GetType().GetProperty("secret")?.GetValue(valueObj, null);
            Assert.False(secretObj is null || string.IsNullOrWhiteSpace(secretObj as string));
            var secret = secretObj as string ?? string.Empty;
            var totp = new Totp(Base32Encoding.ToBytes(secret));
            var code = totp.ComputeTotp();
            controller.Verify2FA(code);
            // Now disable
            var disableResult = controller.Disable2FA(code);
            var ok = Assert.IsType<OkObjectResult>(disableResult);
            Assert.NotNull(ok.Value);
            Assert.Contains("disabled", ok.Value.ToString(), StringComparison.OrdinalIgnoreCase);
            context.Entry(user).Reload();
            Assert.False(user.TwoFactorEnabled);
            Assert.Null(user.TwoFactorSecret);
        }

        [Fact]
        public void Disable2FA_InvalidCode_ReturnsBadRequest()
        {
            var context = CreateInMemoryContext();
            var user = context.Users.Add(new User { Username = "dan", PasswordHash = "hash" }).Entity;
            context.SaveChanges();
            var controller = CreateController(context, user.Id);
            controller.Enable2FA();
            controller.Verify2FA("000000"); // Should fail, but 2FA not enabled
            var disableResult = controller.Disable2FA("000000");
            var badRequest = Assert.IsType<BadRequestObjectResult>(disableResult);
            Assert.NotNull(badRequest.Value);
            Assert.Contains("2FA not enabled.", badRequest.Value.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void GenerateRecoveryCodes_ReturnsCodes()
        {
            var context = CreateInMemoryContext();
            var user = context.Users.Add(new User { Username = "frank", PasswordHash = "hash" }).Entity;
            context.SaveChanges();
            var controller = CreateController(context, user.Id);
            // Enable and verify 2FA
            var enableResult = controller.Enable2FA();
            var valueObj = enableResult.GetType().GetProperty("Value")?.GetValue(enableResult, null);
            var secretObj = valueObj?.GetType().GetProperty("secret")?.GetValue(valueObj, null);
            Assert.False(secretObj is null || string.IsNullOrWhiteSpace(secretObj as string));
            var secret = secretObj as string ?? string.Empty;
            var totp = new Totp(Base32Encoding.ToBytes(secret));
            var code = totp.ComputeTotp();
            controller.Verify2FA(code);
            var result = controller.GenerateRecoveryCodes();
            var ok = Assert.IsType<OkObjectResult>(result);
            var codes = ok.Value?.GetType().GetProperty("recoveryCodes")?.GetValue(ok.Value, null) as System.Collections.IEnumerable;
            Assert.NotNull(codes);
            if (codes != null)
            {
                if (codes is System.Collections.ICollection collection)
                {
                    Assert.Equal(10, collection.Count);
                }
                else
                {
                    Assert.Equal(10, codes.Cast<object>().Count());
                }
            }
        }

        [Fact]
        public void UseRecoveryCode_Success_RemovesCode()
        {
            var context = CreateInMemoryContext();
            var user = context.Users.Add(new User { Username = "gina", PasswordHash = "hash" }).Entity;
            context.SaveChanges();
            var controller = CreateController(context, user.Id);
            // Enable and verify 2FA
            var enableResult = controller.Enable2FA();
            var valueProp = enableResult.GetType().GetProperty("Value")?.GetValue(enableResult, null);
            var secretProp = valueProp?.GetType().GetProperty("secret")?.GetValue(valueProp, null) as string;
            Assert.False(string.IsNullOrWhiteSpace(secretProp));
            var totp = new Totp(Base32Encoding.ToBytes(secretProp));
            var code = totp.ComputeTotp();
            controller.Verify2FA(code);
            var genResult = controller.GenerateRecoveryCodes();
            var okResult = Assert.IsType<OkObjectResult>(genResult);
            var codes = okResult.Value?.GetType().GetProperty("recoveryCodes")?.GetValue(okResult.Value, null) is System.Collections.IEnumerable recoveryCodesObj ? recoveryCodesObj.Cast<string>().ToList() : new List<string>();
            var useResult = controller.UseRecoveryCode(codes.Count > 0 ? codes[0] : string.Empty);
            var ok = Assert.IsType<OkObjectResult>(useResult);
            Assert.NotNull(ok.Value);
            Assert.Contains("accepted", ok.Value.ToString(), System.StringComparison.OrdinalIgnoreCase);
            context.Entry(user).Reload();
            Assert.DoesNotContain(codes[0], user.TwoFactorRecoveryCodes);
        }

        [Fact]
        public void UseRecoveryCode_Invalid_ReturnsBadRequest()
        {
            var context = CreateInMemoryContext();
            var user = context.Users.Add(new User { Username = "gina", PasswordHash = "hash" }).Entity;
            context.SaveChanges();
            var controller = CreateController(context, user.Id);
            // Enable and verify 2FA
            var enableResult = controller.Enable2FA();
            var valueObj = enableResult.GetType().GetProperty("Value")?.GetValue(enableResult, null);
            var secretObj = valueObj?.GetType().GetProperty("secret")?.GetValue(valueObj, null);
            Assert.False(secretObj is null || string.IsNullOrWhiteSpace(secretObj as string));
            var secret = secretObj as string ?? string.Empty;
            var totp = new Totp(Base32Encoding.ToBytes(secret));
            var code = totp.ComputeTotp();
            controller.Verify2FA(code);
            var useResult = controller.UseRecoveryCode("notarealcode");
            var badRequest = Assert.IsType<BadRequestObjectResult>(useResult);
            Assert.NotNull(badRequest.Value);
            Assert.Contains("No recovery codes available.", badRequest.Value.ToString(), System.StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Get2FAStatus_ReturnsEnabledState()
        {
            var context = CreateInMemoryContext();
            var user = context.Users.Add(new User { Username = "hank", PasswordHash = "hash" }).Entity;
            context.SaveChanges();
            var controller = CreateController(context, user.Id);
            // Initially disabled
            var result = controller.Get2FAStatus();
            var ok = Assert.IsType<OkObjectResult>(result);
            var enabledObj = ok.Value?.GetType().GetProperty("enabled")?.GetValue(ok.Value, null);
            Assert.NotNull(enabledObj);
            var enabled = enabledObj is bool enabledBool && enabledBool;
            Assert.False(enabled);
            // Enable
            var enableResult = controller.Enable2FA();
            var valueObj = enableResult.GetType().GetProperty("Value")?.GetValue(enableResult, null);
            var secretObj = valueObj?.GetType().GetProperty("secret")?.GetValue(valueObj, null);
            Assert.False(secretObj is null || string.IsNullOrWhiteSpace(secretObj as string));
            var secret = secretObj as string ?? string.Empty;
            var totp = new Totp(Base32Encoding.ToBytes(secret));
            var code = totp.ComputeTotp();
            controller.Verify2FA(code);
            var result2 = controller.Get2FAStatus();
            var ok2 = Assert.IsType<OkObjectResult>(result2);
            var enabledProp = ok2.Value?.GetType().GetProperty("enabled");
            var enabledValue = enabledProp?.GetValue(ok2.Value, null);
            Assert.NotNull(enabledValue);
            var enabled2 = enabledValue is bool enabledBool2 && enabledBool2;
            Assert.True(enabled2);
        }

        [Fact]
        public void GetRecoveryCodes_ReturnsCodes()
        {
            var context = CreateInMemoryContext();
            var user = context.Users.Add(new User { Username = "ian", PasswordHash = "hash" }).Entity;
            context.SaveChanges();
            var controller = CreateController(context, user.Id);
            // Enable and verify 2FA
            var enableResult = controller.Enable2FA();
            var valueObj = enableResult.GetType().GetProperty("Value")?.GetValue(enableResult, null);
            var secretObj = valueObj?.GetType().GetProperty("secret")?.GetValue(valueObj, null);
            Assert.False(secretObj is null || string.IsNullOrWhiteSpace(secretObj as string));
            var secret = secretObj as string ?? string.Empty;
            var totp = new Totp(Base32Encoding.ToBytes(secret));
            var code = totp.ComputeTotp();
            controller.Verify2FA(code);
            var genResult = controller.GenerateRecoveryCodes();
            var getResult = controller.GetRecoveryCodes();
            var ok = Assert.IsType<OkObjectResult>(getResult);
            var codesObj = ok.Value?.GetType().GetProperty("recoveryCodes")?.GetValue(ok.Value, null);
            Assert.NotNull(codesObj);
            var codes = codesObj as System.Collections.IEnumerable;
            Assert.NotNull(codes);
            Assert.True(codes is System.Collections.ICollection { Count: 10 } || codes.Cast<object>().Count() == 10);
        }

        [Fact]
        public void RegenerateRecoveryCodes_Success()
        {
            var context = CreateInMemoryContext();
            var user = context.Users.Add(new User { Username = "jane", PasswordHash = "hash" }).Entity;
            context.SaveChanges();
            var controller = CreateController(context, user.Id);
            // Enable and verify 2FA
            var enableResult = controller.Enable2FA();
            var valueProp = enableResult.GetType().GetProperty("Value")?.GetValue(enableResult, null);
            var secretProp = valueProp?.GetType().GetProperty("secret")?.GetValue(valueProp, null) as string;
            Assert.False(string.IsNullOrWhiteSpace(secretProp));
            var totp = new Totp(Base32Encoding.ToBytes(secretProp!));
            var code = totp.ComputeTotp();
            controller.Verify2FA(code);
            var regenResult = controller.RegenerateRecoveryCodes(code);
            var ok = Assert.IsType<OkObjectResult>(regenResult);
            var codesObj = ok.Value?.GetType().GetProperty("recoveryCodes")?.GetValue(ok.Value, null);
            Assert.NotNull(codesObj);
            var codes = codesObj as System.Collections.IEnumerable;
            Assert.NotNull(codes);
            Assert.True(codes is System.Collections.ICollection { Count: 10 } || codes.Cast<object>().Count() == 10);
        }
    }
}
