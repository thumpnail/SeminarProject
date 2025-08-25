FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Minimal/ChatMessagingService/ChatMessagingService.csproj", "Minimal/ChatMessagingService/"]
RUN dotnet restore "Minimal/ChatMessagingService/ChatMessagingService.csproj"
COPY . .
WORKDIR "/src/Minimal/ChatMessagingService"
RUN dotnet build "./ChatMessagingService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ChatMessagingService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChatMessagingService.dll"]
