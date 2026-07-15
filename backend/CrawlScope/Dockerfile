FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy the solution file
COPY CrawlScope.sln ./

# Copy all project files keeping the directory structure intact
COPY src/Core/CrawlScope.Domain/CrawlScope.Domain.csproj src/Core/CrawlScope.Domain/
COPY src/Core/CrawlScope.Application/CrawlScope.Application.csproj src/Core/CrawlScope.Application/
COPY src/Infrastructure/CrawlScope.Infrastructure/CrawlScope.Infrastructure.csproj src/Infrastructure/CrawlScope.Infrastructure/
COPY src/Infrastructure/CrawlScope.Persistence/CrawlScope.Persistence.csproj src/Infrastructure/CrawlScope.Persistence/
COPY src/Presentation/CrawlScope.Api/CrawlScope.Api.csproj src/Presentation/CrawlScope.Api/
COPY tests/CrawlScope.Application.Tests/CrawlScope.Application.Tests.csproj tests/CrawlScope.Application.Tests/

# Restore dependencies
RUN dotnet restore

# Copy everything else and build the application
COPY . .
WORKDIR /app/src/Presentation/CrawlScope.Api
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Use ASP.NET runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "CrawlScope.Api.dll"]
