# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY BookShop.API/BookShop.API.csproj ./BookShop.API.csproj
RUN dotnet restore ./BookShop.API.csproj

COPY BookShop.API/ ./BookShop.API/
RUN dotnet publish ./BookShop.API/BookShop.API.csproj -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "BookShop.API.dll"]
