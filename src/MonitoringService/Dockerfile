FROM microsoft/dotnet:2.1-aspnetcore-runtime
WORKDIR /app
COPY . .
ENTRYPOINT ["dotnet", "MonitoringService.dll"]
EXPOSE 5000/tcp
ENV ASPNETCORE_URLS http://*:5000
