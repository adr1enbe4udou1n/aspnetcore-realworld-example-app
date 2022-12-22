FROM mcr.microsoft.com/dotnet/aspnet:7.0
RUN apt-get install -y tzdata

COPY /publish /app
WORKDIR /app

RUN addgroup --gid 2000 app \
    && adduser --uid 1000 --gid 2000 app

RUN chown -R app:app /app
USER app

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080
ENTRYPOINT ["dotnet", "Conduit.WebUI.dll"]
