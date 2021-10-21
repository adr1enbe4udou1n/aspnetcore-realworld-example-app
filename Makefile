.PHONY: publish

run:
	dotnet run -p src/WebUI
watch:
	dotnet watch run -p src/WebUI
fresh:
	dotnet run -p tools/Application.Tools fresh
seed:
	dotnet run -p tools/Application.Tools seed
publish:
	dotnet run -p targets
format:
	dotnet format
test:
	dotnet test -l:"console;verbosity=detailed"
test-watch-app:
	dotnet watch test -p tests/Application.IntegrationTests -l:"console;verbosity=detailed"
test-watch-web:
	dotnet watch test -p tests/WebUI.IntegrationTests
migrations-add:
	dotnet ef migrations add -p src/Infrastructure -s src/WebUI -o Persistence/Migrations $(name)
migrations-remove:
	dotnet ef migrations remove -p src/Infrastructure -s src/WebUI
db-update:
	dotnet ef database update -p src/Infrastructure -s src/WebUI $(name)
