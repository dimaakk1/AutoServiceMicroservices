FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
WORKDIR /src/src/Services/Vehicles/API
RUN dotnet restore AutoserviceVehicle.API.csproj
RUN dotnet publish AutoserviceVehicle.API.csproj -c Release --no-restore -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
EXPOSE 8081
USER app
ENTRYPOINT ["dotnet", "AutoserviceVehicle.API.dll"]
