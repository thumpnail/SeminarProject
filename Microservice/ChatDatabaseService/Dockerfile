FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Minimal/ChatDatabaseService/ChatDatabaseService.csproj", "Minimal/ChatDatabaseService/"]
RUN dotnet restore "Minimal/ChatDatabaseService/ChatDatabaseService.csproj"
COPY . .
WORKDIR "/src/Minimal/ChatDatabaseService"
RUN dotnet build "./ChatDatabaseService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ChatDatabaseService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChatDatabaseService.dll"]
