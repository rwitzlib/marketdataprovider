FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /app

# Copy only the necessary files for the build (you may need to adjust this based on your project structure)
COPY . .
RUN dotnet restore

# Copy the full application and build
COPY . .
RUN dotnet build -c Release

# Test Stage
FROM build AS test
WORKDIR /app
RUN dotnet test --verbosity normal

# Publish Stage
FROM build AS publish
WORKDIR /app
RUN dotnet publish -c Release --no-restore --no-build -o /publish

# Final Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final

EXPOSE 8080
EXPOSE 443

WORKDIR /app

COPY --from=publish /publish .

ENTRYPOINT ["dotnet", "MarketDataProvider.Api.dll"]