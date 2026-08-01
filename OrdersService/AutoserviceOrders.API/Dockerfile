FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Копіюємо все рішення
COPY . .

# Переходимо до API
WORKDIR /src/OrdersService/AutoserviceOrders.API

# Restore
RUN dotnet restore AutoserviceOrders.API.csproj

# Publish
RUN dotnet publish AutoserviceOrders.API.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# ==========================
# RUNTIME
# ==========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "AutoserviceOrders.API.dll"]