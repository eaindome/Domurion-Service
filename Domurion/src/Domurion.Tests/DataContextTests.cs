using Domurion.Data;
using Domurion.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System;
using System.Linq;

namespace Domurion.Tests
{
	public class DataContextTests
	{
		private static DbContextOptions<DataContext> CreateSqliteOptions(SqliteConnection connection)
		{
			return new DbContextOptionsBuilder<DataContext>()
				.UseSqlite(connection)
				.Options;
		}

		[Fact]
		public void Migrations_ApplySuccessfully_AndTablesExist()
		{
			using var connection = new SqliteConnection("Filename=:memory:");
			connection.Open();
			// Enable foreign key constraints
			using (var cmd = connection.CreateCommand())
			{
				cmd.CommandText = "PRAGMA foreign_keys = ON;";
				cmd.ExecuteNonQuery();
			}
			var options = CreateSqliteOptions(connection);
			using (var context = new DataContext(options))
			{
				context.Database.EnsureCreated(); // Or context.Database.Migrate() if you want to test real migrations
				// Check tables exist by inserting and saving
				var user = new User { Username = "test", PasswordHash = "hash" };
				context.Users.Add(user);
				context.SaveChanges();
				Assert.NotEqual(Guid.Empty, user.Id);
				var cred = new Credential { UserId = user.Id, Site = "site", Username = "u", EncryptedPassword = "pw", IntegrityHash = "h" };
				context.Credentials.Add(cred);
				context.SaveChanges();
				Assert.NotEqual(Guid.Empty, cred.Id);
			}
		}

		[Fact]
		public void Constraints_UniqueUsernames_Enforced()
		{
			using var connection = new SqliteConnection("Filename=:memory:");
			connection.Open();
			var options = CreateSqliteOptions(connection);
			using (var context = new DataContext(options))
			{
				context.Database.EnsureCreated();
				var user1 = new User { Username = "unique", PasswordHash = "hash" };
				var user2 = new User { Username = "unique", PasswordHash = "hash2" };
				context.Users.Add(user1);
				context.Users.Add(user2);
				Assert.Throws<DbUpdateException>(() => context.SaveChanges());
			}
		}

		[Fact]
		public void ForeignKeyConstraint_Credential_User_Enforced()
		{
			using var connection = new SqliteConnection("Filename=:memory:");
			connection.Open();
			var options = CreateSqliteOptions(connection);
			using (var context = new DataContext(options))
			{
				context.Database.EnsureCreated();
				// Try to add credential with non-existent user
				var cred = new Credential { UserId = Guid.NewGuid(), Site = "site", Username = "u", EncryptedPassword = "pw", IntegrityHash = "h" };
				context.Credentials.Add(cred);
				Assert.Throws<DbUpdateException>(() => context.SaveChanges());
			}
		}

		// Add a test for seed data if you have any
		// [Fact]
		// public void SeedData_IsPresent()
		// {
		//     // ...
		// }
	}
}
