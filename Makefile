build:
	dotnet build
clean:
	dotnet clean
restore:
	dotnet restore
watch:
	dotnet watch run --project src/WebUI
start:
	dotnet run --project src/WebUI
test-app:
	dotnet watch test --project tests/Application.IntegrationTests
test-web:
	dotnet watch test --project tests/WebUI.IntegrationTests
