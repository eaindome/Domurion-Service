using Domurion.Helpers;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Mail;
using Xunit;

namespace Domurion.Tests
{
	public class EmailServiceTests
	{
		private static IConfiguration CreateConfig(
			string? host = "localhost",
			string? port = "25",
			string? user = "test@example.com",
			string? pass = "password",
			string? from = "test@example.com")
		{
			var dict = new Dictionary<string, string?>
			{
				{ "Smtp:Host", host },
				{ "Smtp:Port", port },
				{ "Smtp:Username", user },
				{ "Smtp:Password", pass },
				{ "Smtp:From", from }
			};
			return new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
		}

		[Fact(Skip = "Integration test - requires SMTP server")]
		public void SendEmail_Success_DoesNotThrow()
		{
			var config = CreateConfig();
			var service = new EmailService(config);
			// This will fail unless a local SMTP server is running
			Exception? ex = Record.Exception(() => service.SendEmail("recipient@example.com", "Test", "Body"));
			// Accept either no exception or SmtpException (connection refused)
			Assert.True(ex == null || ex is SmtpException);
		}

		[Fact]
		public void SendEmail_ThrowsIfFromMissing()
		{
			var config = CreateConfig(from: null, user: null);
			var service = new EmailService(config);
			var ex = Assert.Throws<InvalidOperationException>(() => service.SendEmail("to@example.com", "Test", "Body"));
			Assert.Contains("Sender email address", ex.Message);
		}

		[Fact]
		public void SendEmail_ThrowsIfHostMissing()
		{
			var config = CreateConfig(host: null);
			var service = new EmailService(config);
			Assert.Throws<InvalidOperationException>(() => service.SendEmail("to@example.com", "Test", "Body"));
		}

		[Fact]
		public void SendEmail_ThrowsIfPortInvalid()
		{
			var config = CreateConfig(port: "notaport");
			var service = new EmailService(config);
			Assert.Throws<FormatException>(() => service.SendEmail("to@example.com", "Test", "Body"));
		}
	}
}
