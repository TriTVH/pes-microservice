# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8082


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SyllabusService.API/SyllabusService.API.csproj", "SyllabusService.API/"]
COPY ["SyllabusService.Application/SyllabusService.Application.csproj", "SyllabusService.Application/"]
COPY ["SyllabusService.Infrastructure/SyllabusService.Infrastructure.csproj", "SyllabusService.Infrastructure/"]
RUN dotnet restore "./SyllabusService.API/SyllabusService.API.csproj"
COPY . .
WORKDIR "/src/SyllabusService.API"
RUN dotnet build "./SyllabusService.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SyllabusService.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:8082
ENTRYPOINT ["dotnet", "SyllabusService.API.dll"]