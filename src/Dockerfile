FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS publish
WORKDIR /app

RUN dotnet publish -c Release --no-restore --no-build -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final

EXPOSE 8080
EXPOSE 443

WORKDIR /app

COPY --from=publish /publish .

ENTRYPOINT ["dotnet", "MarketDataProvider.Api.dll"]