FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS publish
WORKDIR /app

ARG PROJECT_DIRECTORY="MarketDataProvider.Api"

COPY . .

RUN dotnet publish -c Release -o ./publish $PROJECT_DIRECTORY

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final

EXPOSE 8080
EXPOSE 443

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "MarketDataProvider.Api.dll"]