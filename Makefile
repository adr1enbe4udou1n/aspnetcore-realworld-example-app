build:
	dotnet build
clean:
	dotnet clean
restore:
	dotnet restore
tool-restore:
	dotnet tool restore
watch:
	dotnet watch run --project src/WebUI
start:
	dotnet run --project src/WebUI
publish:
	dotnet run --project targets
format:
	dotnet tool run dotnet-format
test:
	dotnet test -l:"console;verbosity=detailed"
test-watch-app:
	dotnet watch test --project tests/Application.IntegrationTests -l:"console;verbosity=detailed"
test-watch-web:
	dotnet watch test --project tests/WebUI.IntegrationTests
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
docker-build:
	docker-compose build
docker-run:
	docker-compose up
