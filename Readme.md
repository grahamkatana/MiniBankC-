# MiniBank API

A banking system API built with ASP.NET Core that provides account management and transaction processing capabilities.

## Overview

MiniBank is a RESTful API that allows users to create accounts, manage transactions (deposits, withdrawals, transfers), and view transaction history. The system uses JWT authentication for secure access and SQL Server for data persistence.

## Tech Stack

- **Framework**: ASP.NET Core 8.0
- **Language**: C# 12
- **Database**: SQL Server 2022 (Docker)
- **ORM**: Entity Framework Core
- **Authentication**: JWT Bearer tokens
- **Testing**: xUnit, Moq, FluentAssertions
- **API Documentation**: Swagger/OpenAPI

## Prerequisites

Before running this project, ensure you have the following installed:

- .NET 8 SDK or higher
- Docker Desktop (for SQL Server)
- A code editor (Visual Studio Code, Visual Studio, or Rider)

## Getting Started

### 1. Clone the Repository
```bash
git clone <repository-url>
cd MiniBank
```

### 2. Start SQL Server (Docker)
```bash
docker run -d \
  --name minibank-sqlserver \
  -e "ACCEPT_EULA=Y" \
  -e "MSSQL_SA_PASSWORD=YourStrong@Password123" \
  -p 1433:1433 \
  mcr.microsoft.com/mssql/server:2022-latest
```

Wait about 15 seconds for SQL Server to initialize.

### 3. Configure Connection String

Update `appsettings.json` in the `MiniBank.Api` project:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=minibankdb;User Id=sa;Password=YourStrong@Password123;TrustServerCertificate=True"
  },
  "JWT": {
    "Issuer": "http://localhost:5000",
    "Audience": "http://localhost:5000",
    "SigningKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLongForSecurity123!"
  }
}
```

### 4. Apply Database Migrations
```bash
cd MiniBank.Api
dotnet ef database update
```

This will create the database and all necessary tables.

### 5. Run the Application
```bash
dotnet run
```

The API will start and be available at `http://localhost:5000` (or the port shown in the console).

## Using the API

### Access Documentation

Once the application is running, navigate to:
- Root URL: `http://localhost:5000` (redirects to Swagger)
- Swagger UI: `http://localhost:5000/swagger`

### Testing with HTTP File

The project includes a `requests.http` file for testing endpoints. Use the REST Client extension in VS Code to execute requests directly from this file.

### Basic Workflow

1. Register a new user at `/api/authentication/register`
2. Login to receive a JWT token at `/api/authentication/login`
3. Use the token in the Authorization header for subsequent requests
4. Create an account at `/api/account`
5. Perform transactions (deposit, withdraw, transfer)

## API Endpoints

### Authentication
- `POST /api/authentication/register` - Register new user
- `POST /api/authentication/login` - Login and get JWT token

### Accounts
- `GET /api/account` - Get all user accounts
- `GET /api/account/{id}` - Get account by ID
- `GET /api/account/number/{accountNumber}` - Get account by account number
- `GET /api/account/balance/{accountNumber}` - Get account balance
- `POST /api/account` - Create new account
- `PUT /api/account/{id}` - Update account
- `DELETE /api/account/{id}` - Delete account

### Transactions
- `GET /api/transaction/{id}` - Get transaction by ID
- `GET /api/transaction/account/{accountNumber}` - Get account transactions
- `GET /api/transaction/account/{accountNumber}/range` - Get transactions by date range
- `POST /api/transaction/deposit` - Deposit money
- `POST /api/transaction/withdraw` - Withdraw money
- `POST /api/transaction/transfer` - Transfer between accounts
- `GET /api/transaction/all` - Get all transactions (Admin only)

## Running Tests

The project includes comprehensive unit and integration tests.

### Run All Tests
```bash
dotnet test
```

### Run Tests with Detailed Output
```bash
dotnet test --logger "console;verbosity=detailed"
```

### Run Specific Test Project
```bash
dotnet test MiniBank.Tests/MiniBank.Tests.csproj
```

### Run Tests with Coverage
```bash
dotnet test /p:CollectCoverage=true
```

## Project Structure
```
MiniBank/
├── MiniBank.Api/                   # Main API project
│   ├── Controllers/                # API endpoints
│   ├── Data/                       # DbContext and configurations
│   ├── Dtos/                       # Data transfer objects
│   ├── Interfaces/                 # Service and repository interfaces
│   ├── Models/                     # Domain entities
│   ├── Repository/                 # Data access implementations
│   ├── Services/                   # Business logic
│   └── Program.cs                  # Application entry point
├── MiniBank.Tests/                 # Test project
│   ├── Controllers/                # Controller tests
│   ├── Services/                   # Service tests
│   ├── Repositories/               # Repository tests
│   └── Helpers/                    # Test utilities
├── MiniBank.sln                    # Solution file
├── Readme.md                       # This file
└── requests.http                   # API test requests
```

## Database

### Entities

The system uses three main entities:

- **Users**: User accounts with authentication (managed by ASP.NET Identity)
- **Accounts**: Bank accounts with balance and account number
- **Transactions**: Records of deposits, withdrawals, and transfers

### Migrations

Create a new migration after model changes:
```bash
dotnet ef migrations add MigrationName --output-dir db/migrations
```

Apply migrations:
```bash
dotnet ef database update
```

Rollback to previous migration:
```bash
dotnet ef database update PreviousMigrationName
```

Remove last migration (if not applied):
```bash
dotnet ef migrations remove
```

## Common Commands

### Build
```bash
dotnet build
```

### Run in Development
```bash
dotnet run --project MiniBank.Api
```

### Run with Watch (auto-reload)
```bash
dotnet watch run --project MiniBank.Api
```

### Clean Build Artifacts
```bash
dotnet clean
```

### Restore NuGet Packages
```bash
dotnet restore
```

## Docker Commands

### Start SQL Server
```bash
docker start minibank-sqlserver
```

### Stop SQL Server
```bash
docker stop minibank-sqlserver
```

### View SQL Server Logs
```bash
docker logs minibank-sqlserver
```

### Remove SQL Server Container
```bash
docker rm -f minibank-sqlserver
```

## Authentication

The API uses JWT (JSON Web Tokens) for authentication. After logging in, include the token in the Authorization header:
```
Authorization: Bearer YOUR_JWT_TOKEN_HERE
```

Tokens are valid for the duration specified in the JWT configuration (default: configurable in appsettings.json).

## Development

### Adding New Features

1. Create or modify models in `Models/`
2. Create migration: `dotnet ef migrations add FeatureName`
3. Apply migration: `dotnet ef database update`
4. Add DTOs in `Dtos/`
5. Create interface in `Interfaces/`
6. Implement repository in `Repository/`
7. Implement service in `Services/`
8. Create controller in `Controllers/`
9. Write tests in `MiniBank.Tests/`
10. Update `requests.http` with new endpoints

### Code Style

- Use PascalCase for public members
- Use camelCase for local variables
- Use async/await for all database operations
- Include XML comments for public APIs
- Write tests for new features

## Troubleshooting

### Database Connection Issues

If you cannot connect to the database:
1. Verify SQL Server container is running: `docker ps`
2. Check connection string in `appsettings.json`
3. Ensure SQL Server has finished starting (check logs)

### Migration Issues

If migrations fail:
1. Delete the database: `dotnet ef database drop`
2. Delete all migration files in `db/migrations/`
3. Recreate migrations: `dotnet ef migrations add InitialCreate --output-dir db/migrations`
4. Apply migrations: `dotnet ef database update`

### Port Already in Use

If port 5000 is already in use, change it in `Properties/launchSettings.json`.

## Security Notes

- Never commit `appsettings.json` with real credentials to version control
- Use environment variables for production secrets
- Change the default JWT signing key before deployment
- Use strong passwords for database accounts
- Enable HTTPS in production

## Contributing

1. Create a feature branch
2. Make your changes
3. Write or update tests
4. Ensure all tests pass
5. Submit a pull request

## License

This project is for educational purposes.

## Contact

For questions or issues, please open an issue on the repository.