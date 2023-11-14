FROM mcr.microsoft.com/dotnet/aspnet:8.0
USER app

COPY --chown=app:app /publish /app
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080
ENTRYPOINT ["dotnet", "Conduit.WebUI.dll"]
