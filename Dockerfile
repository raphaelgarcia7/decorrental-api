FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["DecorRental.sln", "./"]
COPY ["DecorRental.Domain/DecorRental.Domain.csproj", "DecorRental.Domain/"]
COPY ["DecorRental.Application/DecorRental.Application.csproj", "DecorRental.Application/"]
COPY ["DecorRental.Infrastructure/DecorRental.Infrastructure.csproj", "DecorRental.Infrastructure/"]
COPY ["DecorRental.Api/DecorRental.API.csproj", "DecorRental.Api/"]

RUN dotnet restore "DecorRental.Api/DecorRental.API.csproj"

COPY . .
WORKDIR /src/DecorRental.Api
RUN dotnet publish "DecorRental.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "DecorRental.Api.dll"]
