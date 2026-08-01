FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY . .

WORKDIR /src/AutoServiceUsers.API

RUN dotnet restore AutoServiceUsers.API.csproj

RUN dotnet publish AutoServiceUsers.API.csproj \
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

ENTRYPOINT ["dotnet", "AutoServiceUsers.API.dll"]
