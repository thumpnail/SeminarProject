@echo off

echo Parameters: [lite, memory, mock]

start "" ".\ChatHistoryService.exe"
start "" ".\ChatDatabaseService.exe" memory
start "" ".\ChatMessagingService.exe"
start "" ".\ChatApp.Server.exe" memory

echo Parameters: [lite, memory]
.\Chat.Tests.exe memory
pause