# ![RealWorld Example App](logo.png)

ASP.NET Core codebase containing real world examples (CRUD, auth, advanced patterns, etc) that adheres to the [RealWorld](https://github.com/gothinkster/realworld-example-apps) spec and API.

## [RealWorld](https://github.com/gothinkster/realworld)

This codebase was created to demonstrate a fully fledged fullstack application built with ASP.NET Core (with Feature orientation) including CRUD operations, authentication, routing, pagination, and more.

We've gone to great lengths to adhere to the ASP.NET Core community styleguides & best practices.

For more information on how to this works with other frontends/backends, head over to the [RealWorld](https://github.com/gothinkster/realworld) repo.

Set valid PostgreSQL connection by setting `ConnectionStrings__DefaultConnection` variable environment before next commands.

### Validate API with Newman

```sh
dotnet run -p src/WebUI
newman run postman.json --global-var "APIURL=http://localhost:5000" --global-var="USERNAME=johndoe" --global-var="EMAIL=john.doe@example.com" --global-var="PASSWORD=password"
```

### Useful commands

```sh
dotnet run -p targets # Execute all format + test suite + publish pipeline
dotnet run -p tools/Application.Tools fresh # Wipe all database data
dotnet run -p tools/Application.Tools seed # Seed random data via Bogus
```
