FROM microsoft/dotnet:2.0-runtime
ENTRYPOINT ["dotnet", "MonitoringService.dll"]
ARG source=.
WORKDIR /app
COPY $source .

EXPOSE 5000/tcp

ENV ASPNETCORE_URLS http://*:5000
