.PHONY: publish

run:
	dotnet run --project src/Conduit.WebUI
watch:
	@cd src/Conduit.WebUI && dotnet watch
migrate:
	@cd tools/Conduit.Tools && dotnet run db migrate
fresh:
	@cd tools/Conduit.Tools && dotnet run db fresh
seed:
	@cd tools/Conduit.Tools && dotnet run db seed
format:
	dotnet format
test:
	dotnet test -l:"console;verbosity=detailed"
test-watch-app:
	dotnet watch test --project tests/Application.IntegrationTests -l:"console;verbosity=detailed"
test-watch-web:
	dotnet watch test --project tests/Conduit.IntegrationTests
migrations-add:
	dotnet ef migrations add --project src/Conduit.Infrastructure -s src/Conduit.WebUI -o Persistence/Migrations $(name)
migrations-remove:
	dotnet ef migrations remove --project src/Conduit.Infrastructure -s src/Conduit.WebUI
db-update:
	dotnet ef database update --project src/Conduit.Infrastructure -s src/Conduit.WebUI $(name)
