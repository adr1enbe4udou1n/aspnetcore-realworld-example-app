FROM mcr.microsoft.com/dotnet/aspnet:6.0
RUN apt-get install -y tzdata

COPY /publish /app
WORKDIR /app

ENTRYPOINT ["dotnet", "WebUI.dll"]
