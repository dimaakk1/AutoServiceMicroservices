FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY . .

WORKDIR /src/AutoServiceTelegram.API

RUN dotnet restore AutoServiceTelegram.API.csproj

RUN dotnet publish AutoServiceTelegram.API.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet","AutoServiceTelegram.API.dll"]