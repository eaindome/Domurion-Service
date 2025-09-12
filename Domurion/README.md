# Domurion Service

Domurion is a secure credential management and authentication service built with ASP.NET Core. This repository contains the main service, supporting libraries, and end-to-end tests.

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- [Docker](https://www.docker.com/) (for running with Docker Compose)
- (Optional) [Visual Studio 2022+](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
- (Optional) [PostgreSQL](https://www.postgresql.org/) or [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) if not using Docker

## Getting Started

### 1. Clone the Repository

```powershell
git clone <your-fork-or-repo-url>
cd Domurion
```
  
### 2. Local Development (without Docker)

1. Navigate to the main project directory:

   ```powershell
   cd src/Domurion
   ```

2. Restore dependencies:

   ```powershell
   dotnet restore
   ```

3. Run database migrations (if needed):

   ```powershell
   dotnet ef database update
   ```

4. Start the API:

   ```powershell
   dotnet run
   ```

5. The API will be available at `https://localhost:5001` or `http://localhost:5000` by default.

### 3. Running with Docker Compose

1. From the root directory:

   ```powershell
   docker-compose up --build
   ```

2. The API and any dependent services will be started in containers.

### 4. Running Tests

1. Navigate to the test project:

   ```powershell
   cd src/Domurion.Tests
   ```

2. Run all tests:

   ```powershell
   dotnet test
   ```

## Configuration

Main configuration files:

- `src/Domurion/appsettings.json` (base settings)
- `src/Domurion/appsettings.Development.json` (development overrides)
- `src/Domurion/appsettings.Production.json` (production overrides, optional)

For local development (non-Docker, non-production), you can use [user secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) to securely store sensitive settings:

1. Navigate to `src/Domurion`
2. Run: `dotnet user-secrets init`
3. Run: `dotnet user-secrets set "Jwt:Key" "your-secret-key"`

Environment variables can override any setting (recommended for secrets and connection strings).
Example environment variables:

- `ASPNETCORE_ENVIRONMENT=Development`
- `ConnectionStrings__DefaultConnection=...`
- `Jwt__Key=...`
- `Jwt__Issuer=...`
- `Authentication__Google__ClientId=...`
- `Authentication__Google__ClientSecret=...`

For Docker, you can set environment variables in `docker-compose.yml`.
To set environment variables manually:

- **Windows (PowerShell):** `$env:Jwt__Key="your-secret-key"`
- **Linux/macOS (bash):** `export Jwt__Key="your-secret-key"`

## Project Structure

```text
Domurion/
├── docker-compose.yml             # Docker Compose setup for API and dependencies
├── Domurion.sln                   # Solution file
└── src/
   ├── Domurion/                  # Main API project
   │   ├── Controllers/           # API controllers (Auth, Users, Vault, etc.)
   │   │   ├── AuthController.cs
   │   │   ├── GoogleLinkController.cs
   │   │   ├── PreferencesController.cs
   │   │   ├── SupportController.cs
   │   │   ├── TwoFactorController.cs
   │   │   ├── UsersController.cs
   │   │   └── VaultController.cs
   │   ├── Data/                  # Entity Framework DbContext and migrations
   │   │   ├── DataContext.cs
   │   ├── Dtos/                  # Data transfer objects
   │   │   ├── ImportCredentialDto.cs
   │   │   └── UserDto.cs
   │   ├── Helpers/               # Utility and helper classes
   │   │   ├── AuditLogger.cs
   │   │   ├── CryptoHelper.cs
   │   │   ├── EmailService.cs
   │   │   ├── ErrorHandlingMiddleware.cs
   │   │   ├── Helper.cs
   │   │   └── JwtHelper.cs
   │   ├── Migrations/            # EF Core migrations
   │   │   ├── <timestamp>_InitialCreate.cs
   │   │   ├── <timestamp>_InitialCreate.Designer.cs
   │   │   ├── ...
   │   │   └── DataContextModelSnapshot.cs
   │   ├── Models/                # Entity models
   │   │   ├── AuditLog.cs
   │   │   ├── Credential.cs
   │   │   ├── PasswordHistory.cs
   │   │   ├── Preferences.cs
   │   │   ├── SupportRequest.cs
   │   │   └── User.cs
   │   ├── Services/              # Business logic and services
   │   ├── appsettings.json       # Main configuration
   │   ├── appsettings.Development.json
   │   ├── Program.cs             # Entry point
   │   └── Domurion.csproj        # Project file
   └── Domurion.Tests/            # Test project (unit and e2e tests)
      ├── AuthControllerTests.cs
      ├── AuthProtectedTests.cs
      ├── DataContextTests.cs
      ├── e2eTests.cs            # End-to-end tests
      ├── EmailServiceTests.cs
      ├── GlobalErrorHandlingTests.cs
      ├── GoogleLinkControllerTests.cs
      ├── HelpersTests.cs
      ├── JwtHelperTests.cs
      ├── ModelsTests.cs
      ├── PasswordVaultServiceTests.cs
      ├── PreferencesControllerTests.cs
      ├── SupportControllerTests.cs
      ├── TwoFactorControllerTests.cs
      ├── UnitTest1.cs
      ├── UsersControllerTests.cs
      ├── UserServiceTests.cs
      ├── VaultControllerTests.cs
      ├── Domurion.Tests.csproj   # Test project file
      └── ...                    # Other test files and folders
```

## Notes

- Some end-to-end (e2e) tests are skipped by default. See `e2eTests.cs` for details.
- For Google OAuth flows, additional setup or mocking is required for automated tests.
- The API uses JWT for authentication. Default keys in test configs are for development only—replace in production.
- Database is configured via connection string. For local development, you can use SQLite, PostgreSQL, or SQL Server.
- To add new migrations:
  1. Navigate to `src/Domurion`
  2. Run: `dotnet ef migrations add <MigrationName>`
  3. Run: `dotnet ef database update`
- For troubleshooting, check logs in the console or configure logging in `appsettings.json`.
- If you encounter port conflicts, change the ports in `launchSettings.json` or Docker Compose.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
