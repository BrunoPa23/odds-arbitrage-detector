# Imagen de la API: se compila con el SDK y corre solo con el runtime de ASP.NET Core
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restaurar primero, solo con el csproj, para reutilizar la cache de capas
COPY OddsArbitrage.Api.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

# Usuario sin privilegios que trae la imagen; escucha en el puerto 8080 por defecto
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "OddsArbitrage.Api.dll"]
