# Domurion Service

Domurion is a secure credential management and authentication service built with ASP.NET Core. This repository contains the main service, supporting libraries, and end-to-end tests.

## Prerequisites
- [.NET 7 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
- [Docker](https://www.docker.com/) (for running with Docker Compose)
- (Optional) [Visual Studio 2022+](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

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
- App settings are in `src/Domurion/appsettings.json` and `appsettings.Development.json`.
- Environment variables can override settings for secrets and connection strings.

## Project Structure
- `src/Domurion/` - Main API project
- `src/Domurion.Tests/` - Test project (unit and e2e tests)
- `docker-compose.yml` - Docker Compose setup

## Notes
- Some e2e tests are skipped by default. See `e2eTests.cs` for details.
- For Google OAuth flows, additional setup or mocking is required for automated tests.

## License
MIT (or specify your license here)
