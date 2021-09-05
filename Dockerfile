#build container
FROM mcr.microsoft.com/dotnet/sdk:5.0 as build

WORKDIR /build
COPY . .
RUN dotnet run -p targets

#runtime container
FROM mcr.microsoft.com/dotnet/aspnet:5.0
RUN apt-get install -y tzdata

COPY --from=build /build/publish /app
WORKDIR /app

EXPOSE 5000

ENTRYPOINT ["dotnet", "WebUI.dll"]
