echo Parameters: [lite, memory, mock]

start "" ".\ChatHistoryService.exe"
start "" ".\ChatDatabaseService.exe"
start "" ".\ChatMessagingService.exe"
start "" ".\ChatApp.Server.exe"

.\Chat.Tests.exe mock
pause