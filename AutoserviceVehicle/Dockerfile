FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY AutoserviceVehicle/AutoserviceVehicle.API.csproj AutoserviceVehicle/
COPY AutoserviceVehicle.BLL/AutoserviceVehicle.BLL.csproj AutoserviceVehicle.BLL/
COPY AutoserviceVehicle.DAL/AutoserviceVehicle.DAL.csproj AutoserviceVehicle.DAL/
COPY AppHostt/AppHostt.ServiceDefaults/AppHostt.ServiceDefaults.csproj AppHostt/AppHostt.ServiceDefaults/
RUN dotnet restore AutoserviceVehicle/AutoserviceVehicle.API.csproj
COPY AutoserviceVehicle/ AutoserviceVehicle/
COPY AutoserviceVehicle.BLL/ AutoserviceVehicle.BLL/
COPY AutoserviceVehicle.DAL/ AutoserviceVehicle.DAL/
COPY AppHostt/AppHostt.ServiceDefaults/ AppHostt/AppHostt.ServiceDefaults/
RUN dotnet publish AutoserviceVehicle/AutoserviceVehicle.API.csproj -c Release --no-restore -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
USER app
ENTRYPOINT ["dotnet", "AutoserviceVehicle.API.dll"]
