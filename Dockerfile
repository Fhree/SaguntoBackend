FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Sagunto.API/Sagunto.API.csproj", "Sagunto.API/"]
COPY ["Sagunto.Application/Sagunto.Application.csproj", "Sagunto.Application/"]
COPY ["Sagunto.Domain/Sagunto.Domain.csproj", "Sagunto.Domain/"]
COPY ["Sagunto.Infrastructure/Sagunto.Infrastructure.csproj", "Sagunto.Infrastructure/"]

RUN dotnet restore "Sagunto.API/Sagunto.API.csproj"

COPY . .
WORKDIR "/src/Sagunto.API"
RUN dotnet publish "Sagunto.API.csproj" -c Release --no-restore -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Sagunto.API.dll"]