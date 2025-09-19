# Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files (the main API + related class libraries)
COPY ["AuthService.API/AuthService.API.csproj", "AuthService.API/"]
COPY ["Auth.Repositories/Auth.Infrastructure.csproj", "Auth.Repositories/"]
COPY ["Auth.Domain/Auth.Domain.csproj", "Auth.Domain/"]
COPY ["Auth.Services/Auth.Application.csproj", "Auth.Services/"]

# Restore dependencies
RUN dotnet restore "AuthService.API/AuthService.API.csproj"

# Copy everything and build
COPY . .
WORKDIR "/src/AuthService.API"
RUN dotnet build "AuthService.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "AuthService.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final runtime stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AuthService.API.dll"]
