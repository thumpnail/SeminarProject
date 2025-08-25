FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Minimal/ChatHistoryService/ChatHistoryService.csproj", "Minimal/ChatHistoryService/"]
RUN dotnet restore "Minimal/ChatHistoryService/ChatHistoryService.csproj"
COPY . .
WORKDIR "/src/Minimal/ChatHistoryService"
RUN dotnet build "./ChatHistoryService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ChatHistoryService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChatHistoryService.dll"]
