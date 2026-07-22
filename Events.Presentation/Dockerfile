FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Events.Presentation/Events.Presentation.csproj", "Events.Presentation/"]
COPY ["Events.Application/Events.Application.csproj", "Events.Application/"]
COPY ["Events.Domain/Events.Domain.csproj", "Events.Domain/"]
COPY ["Events.Infrastructure/Events.Infrastructure.csproj", "Events.Infrastructure/"]
COPY ["Kafka.Contracts/Kafka.Contracts.csproj", "Kafka.Contracts/"]
RUN dotnet restore "Events.Presentation/Events.Presentation.csproj"
COPY . .
WORKDIR "/src/Events.Presentation"
RUN dotnet build "./Events.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Events.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Events.Presentation.dll"]
