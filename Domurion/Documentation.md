# Domurion Service Documentation

## Table of Contents

- [Domurion Service Documentation](#domurion-service-documentation)
  - [Table of Contents](#table-of-contents)
  - [Overview](#overview)
  - [Architecture](#architecture)
    - [Project Structure](#project-structure)
  - [API Endpoints](#api-endpoints)
    - [User Management](#user-management)
    - [Two-Factor Authentication (2FA)](#two-factor-authentication-2fa)
    - [Vault \& Credentials](#vault--credentials)
    - [Preferences \& Support](#preferences--support)
    - [Google OAuth](#google-oauth)
  - [Authentication \& Security](#authentication--security)
  - [Configuration](#configuration)
  - [Database \& Migrations](#database--migrations)
  - [Testing](#testing)
  - [Deployment](#deployment)
  - [Troubleshooting \& FAQ](#troubleshooting--faq)
  - [Contributing](#contributing)
  - [License](#license)

---

## Overview

Domurion is a secure credential management and authentication service built with ASP.NET Core. It provides user registration, login, JWT-based authentication, two-factor authentication (2FA), Google OAuth integration, and a password vault. The project is designed for extensibility, security, and ease of deployment.

## Architecture

- **Backend:** ASP.NET Core Web API (C#)
- **Database:** Entity Framework Core (supports SQLite, PostgreSQL, SQL Server)
- **Authentication:** JWT, 2FA (TOTP), Google OAuth
- **Testing:** xUnit, in-memory database for tests
- **Containerization:** Docker & Docker Compose

### Project Structure

See the `README.md` for a detailed directory and file structure.

## API Endpoints

### User Management

- `POST /api/users/register` — Register a new user
- `POST /api/users/login` — Login and receive JWT
- `GET /api/users/me` — Get current user info (auth required)
- `POST /api/users/link-google` — Link Google account (auth required)

### Two-Factor Authentication (2FA)

- `POST /api/2fa/enable` — Enable 2FA for user
- `POST /api/2fa/verify` — Verify 2FA code
- `POST /api/2fa/disable` — Disable 2FA

### Vault & Credentials

- `GET /api/vault` — Get all credentials (auth required)
- `POST /api/vault` — Add a credential
- `PUT /api/vault/{id}` — Update a credential
- `DELETE /api/vault/{id}` — Delete a credential
- `POST /api/vault/import` — Import credentials

### Preferences & Support

- `GET /api/preferences` — Get user preferences
- `PUT /api/preferences` — Update preferences
- `POST /api/support` — Submit a support request

### Google OAuth

- `POST /api/users/link-google` — Link Google account
- `POST /api/users/login-google` — Login with Google

> **Note:** Some endpoints may require authentication or specific roles. See controller code for details.

## Authentication & Security

- **JWT Authentication:** All protected endpoints require a valid JWT in the `Authorization: Bearer <token>` header.
- **2FA:** Users can enable TOTP-based 2FA for additional security.
- **Password Hashing:** Passwords are securely hashed and salted.
- **Google OAuth:** Users can link or login with Google accounts.
- **Audit Logging:** Sensitive actions are logged for auditing.
- **CORS:** Configurable in `Startup.cs`/`Program.cs`.

## Configuration

- Main config files: `appsettings.json`, `appsettings.Development.json`, `appsettings.Production.json`
- Environment variables override config (see README for examples)
- User secrets supported for local dev: `dotnet user-secrets ...`
- Docker Compose sets environment variables for containers

## Database & Migrations

- Uses Entity Framework Core for ORM and migrations
- Migrations are in `src/Domurion/Migrations/`
- To add a migration:
  1. `cd src/Domurion`
  2. `dotnet ef migrations add <MigrationName>`
  3. `dotnet ef database update`
- For tests, an in-memory database is used

## Testing

- All tests are in `src/Domurion.Tests/`
- Run tests: `dotnet test src/Domurion.Tests`
- e2e tests use in-memory DB and test server
- Some e2e tests are skipped by default (see `e2eTests.cs`)

## Deployment

- Recommended: Use Docker Compose for deployment
- Set production secrets via environment variables or secrets manager
- Use `appsettings.Production.json` for production-specific config
- Expose only necessary ports (default: 5000/5001)
- Monitor logs and audit logs for security

## Troubleshooting & FAQ

- **Port conflicts:** Change ports in `launchSettings.json` or Docker Compose
- **DB connection issues:** Check connection string and DB server
- **JWT errors:** Ensure correct key/issuer in config
- **Google OAuth issues:** Verify client ID/secret and redirect URIs
- **Migrations:** If schema changes, add and apply new migrations

## Contributing

1. Fork the repo and create a feature branch
2. Follow code style and add tests for new features
3. Open a pull request with a clear description
4. Ensure all tests pass before submitting

## License

MIT License. See the [LICENSE](LICENSE) file for details.
