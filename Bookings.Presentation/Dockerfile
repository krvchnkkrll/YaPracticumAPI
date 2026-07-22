FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Build.props", "Directory.Packages.props", "global.json", "./"]
COPY ["Bookings.Presentation/Bookings.Presentation.csproj", "Bookings.Presentation/"]
COPY ["Bookings.Domain/Bookings.Domain.csproj", "Bookings.Domain/"]
COPY ["Bookings.Application/Bookings.Application.csproj", "Bookings.Application/"]
COPY ["Kafka.Contracts/Kafka.Contracts.csproj", "Kafka.Contracts/"]
COPY ["Bookings.Infrastructure/Bookings.Infrastructure.csproj", "Bookings.Infrastructure/"]
RUN dotnet restore "Bookings.Presentation/Bookings.Presentation.csproj"
COPY . .
WORKDIR "/src/Bookings.Presentation"
RUN dotnet build "./Bookings.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Bookings.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Bookings.Presentation.dll"]
