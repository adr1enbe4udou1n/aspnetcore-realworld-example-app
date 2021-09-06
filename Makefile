build:
	docker-compose build
run:
	docker-compose up
watch:
	dotnet watch run -p src/WebUI
start:
	dotnet run -p src/WebUI
publish:
	dotnet run -p targets
format:
	dotnet tool run dotnet-format
test:
	dotnet test -l:"console;verbosity=detailed"
test-watch-app:
	dotnet watch test -p tests/Application.IntegrationTests -l:"console;verbosity=detailed"
test-watch-web:
	dotnet watch test -p tests/WebUI.IntegrationTests
migration-add:
	dotnet ef migrations add -p src/Infrastructure -s src/WebUI -o Persistence/Migrations $(name)
migration-remove:
	dotnet ef migrations remove -p src/Infrastructure -s src/WebUI
db-update:
	dotnet ef database update -p src/Infrastructure -s src/WebUI $(name)
fresh:
	dotnet run -p tools/Application.Tools fresh
seed:
	dotnet run -p tools/Application.Tools seed
