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
migration-add:
	dotnet ef migrations add -p src/Infrastructure -s src/WebUI -o Persistence/Migrations $(name)
migration-remove:
	dotnet ef migrations remove -p src/Infrastructure -s src/WebUI
db-update:
	dotnet ef database update -p src/Infrastructure -s src/WebUI $(name)
seed:
	dotnet run -p tools/Application.Tools seed
