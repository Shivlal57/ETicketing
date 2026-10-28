# ETicketing API

A RESTful API built with **ASP.NET Core .NET 8** for an e-ticketing system. The application uses SQL Server for data storage and Swagger for API documentation and testing.

## Tech Stack

- C# and .NET 8
- ASP.NET Core Web API
- SQL Server
- Entity Framework Core
- Docker
- Swagger

## Prerequisites

Install the following before running the application:

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- SQL Server
- [SQL Server Management Studio (SSMS)](https://learn.microsoft.com/en-us/ssms/install/install)

## Project Structure

```text
ETicketing/
??? ETicketing.sln
??? Dockerfile
??? .dockerignore
??? README.md
??? src/
    ??? ETicketing.Api/
        ??? ETicketing.Api.csproj
```

## Configuration

The application reads its database connection string from the `ConnectionStrings:DefaultConnection` configuration key.

For environment variables, ASP.NET Core uses double underscores (`__`) to represent configuration sections.

The corresponding environment variable is:

```text
ConnectionStrings__DefaultConnection
```

Do not commit database passwords or other secrets to source control.

### SQL Server connection examples

When running the API directly on Windows:

```text
Server=localhost,1433;Database=ETicketingDb;User Id=YOUR_SQL_USERNAME;Password=YOUR_SQL_PASSWORD;TrustServerCertificate=True;
```

When running the API inside Docker while SQL Server runs on the Windows host:

```text
Server=host.docker.internal,1433;Database=ETicketingDb;User Id=YOUR_SQL_USERNAME;Password=YOUR_SQL_PASSWORD;TrustServerCertificate=True;
```

Replace the example username and password with your own credentials. Ensure SQL Server TCP/IP is enabled and that the configured port matches your SQL Server instance.

## Run Locally

1. Open a terminal in the solution root.
2. Configure the database connection string as a user environment variable, or use .NET User Secrets for local development.
3. Restore dependencies:

   ```powershell
   dotnet restore ETicketing.sln
   ```

4. Run the API project:

   ```powershell
   dotnet run --project src/ETicketing.Api/ETicketing.Api.csproj
   ```

5. Open the Swagger URL shown in the terminal output. In Development, it is commonly available at:

   ```text
   http://localhost:PORT/swagger
   ```

   Replace `PORT` with the actual port printed by the application.

## Run with Docker

Make sure Docker Desktop is running.

### 1. Build the image

Run this command from the solution root:

```powershell
docker build -t eticketing-api .
```

### 2. Configure the database connection

Set a connection string in your PowerShell session. Use `host.docker.internal` because SQL Server is running on the Windows host:

```powershell
$conn = "Server=host.docker.internal,1433;Database=ETicketingDb;User Id=YOUR_SQL_USERNAME;Password=YOUR_SQL_PASSWORD;TrustServerCertificate=True;"
```

Replace the placeholder credentials before running the container.

### 3. Start the container

```powershell
docker run -d --name eticketing-api -p 8080:8080 `
  -e "ASPNETCORE_ENVIRONMENT=Development" `
  -e "ConnectionStrings__DefaultConnection=$conn" `
  eticketing-api
```

### 4. Open Swagger

Visit:

```text
http://localhost:8080/swagger
```

### 5. View logs

```powershell
docker logs eticketing-api
```

Follow logs continuously:

```powershell
docker logs -f eticketing-api
```

### 6. Stop and remove the container

```powershell
docker stop eticketing-api
docker rm eticketing-api
```

To start it again after stopping, use `docker start eticketing-api`.

## Troubleshooting

### Cannot connect to SQL Server

- Confirm the SQL Server service is running.
- Enable TCP/IP in SQL Server Configuration Manager.
- Confirm the SQL Server TCP port is correct.
- Use `host.docker.internal` from inside Docker instead of `localhost`.
- Verify the database name and SQL Server credentials.
- Check the container logs for the underlying exception.

### Swagger does not open

- Confirm the container is running:

  ```powershell
  docker ps
  ```

- Check the application logs:

  ```powershell
  docker logs eticketing-api
  ```

- Confirm port `8080` is mapped using `-p 8080:8080`.
- Verify Swagger is enabled for the selected environment.

## Security Notes

- Never commit passwords, API keys, or production connection strings.
- Keep `.env` files and local secret files out of source control.
- Use .NET User Secrets for local development when running outside Docker.
- Use an appropriate secret manager or deployment secret configuration for production.
- Avoid using Development settings in production.

