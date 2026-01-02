# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["MiniBank.sln", "./"]
COPY ["MiniBank.Api/MiniBank.Api.csproj", "MiniBank.Api/"]
COPY ["MiniBank.Tests/MiniBank.Tests.csproj", "MiniBank.Tests/"]

# Restore dependencies
RUN dotnet restore "MiniBank.Api/MiniBank.Api.csproj"

# Copy everything else
COPY . .

# Build
WORKDIR "/src/MiniBank.Api"
RUN dotnet build "MiniBank.Api.csproj" -c Release -o /app/build

# Publish
RUN dotnet publish "MiniBank.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Copy published files
COPY --from=build /app/publish .

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the application
ENTRYPOINT ["dotnet", "MiniBank.Api.dll"]