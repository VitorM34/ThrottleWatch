# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY global.json Directory.Build.props ThrottleWatch.slnx ./
COPY src/ThrottleWatch.Dashboard/ThrottleWatch.Dashboard.csproj src/ThrottleWatch.Dashboard/

RUN dotnet restore src/ThrottleWatch.Dashboard/ThrottleWatch.Dashboard.csproj

COPY src/ThrottleWatch.Dashboard/ src/ThrottleWatch.Dashboard/

RUN dotnet publish src/ThrottleWatch.Dashboard/ThrottleWatch.Dashboard.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ThrottleWatch.Dashboard.dll"]
