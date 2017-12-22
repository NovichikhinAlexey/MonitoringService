FROM microsoft/aspnetcore:2.0
WORKDIR /app
COPY . .
ENTRYPOINT ["dotnet", "MonitoringService.dll"]
EXPOSE 5000/tcp
ENV ASPNETCORE_URLS http://*:5000
