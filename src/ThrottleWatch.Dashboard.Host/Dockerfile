# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY global.json Directory.Build.props ThrottleWatch.slnx ./
COPY src/ThrottleWatch.Dashboard/ src/ThrottleWatch.Dashboard/
COPY src/ThrottleWatch.Dashboard.Host/ src/ThrottleWatch.Dashboard.Host/

RUN dotnet restore src/ThrottleWatch.Dashboard.Host/ThrottleWatch.Dashboard.Host.csproj

RUN dotnet publish src/ThrottleWatch.Dashboard.Host/ThrottleWatch.Dashboard.Host.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV DataProtection__KeysPath=/keys

RUN mkdir -p /keys

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ThrottleWatch.Dashboard.Host.dll"]
