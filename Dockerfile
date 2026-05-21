# 1. Etapa de Construcción (Usamos el SDK)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiamos los archivos de proyecto de TODAS tus capas para optimizar la caché
COPY ["Sagunto.API/Sagunto.API.csproj", "Sagunto.API/"]
COPY ["Sagunto.Application/Sagunto.Application.csproj", "Sagunto.Application/"]
COPY ["Sagunto.Domain/Sagunto.Domain.csproj", "Sagunto.Domain/"]
COPY ["Sagunto.Infrastructure/Sagunto.Infrastructure.csproj", "Sagunto.Infrastructure/"]

# Restauramos dependencias
RUN dotnet restore "Sagunto.API/Sagunto.API.csproj"

# Copiamos el resto del código y compilamos
COPY . .
WORKDIR "/src/Sagunto.API"
RUN dotnet publish "Sagunto.API.csproj" -c Release -o /app/publish

# 2. Etapa de Ejecución (Usamos solo el Runtime, pesa muchísimo menos)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080

# Traemos los archivos compilados desde la etapa anterior
COPY --from=build /app/publish .

# Le decimos a Docker cómo arrancar tu API
ENTRYPOINT ["dotnet", "Sagunto.API.dll"]