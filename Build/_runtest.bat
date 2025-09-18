@echo off

echo Parameters: [lite, memory, mock]

start "" ".\ChatHistoryService.exe"
start "" ".\ChatDatabaseService.exe" lite
start "" ".\ChatMessagingService.exe"
start "" ".\ChatApp.Server.exe" lite

echo Parameters: [lite, memory]
.\Chat.Tests.exe memory
pause