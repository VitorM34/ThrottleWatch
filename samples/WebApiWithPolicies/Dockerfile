# syntax=docker/dockerfile:1
# Sample API with ASP.NET rate limiting + ThrottleWatch.Client

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY global.json Directory.Build.props ThrottleWatch.slnx ./
COPY src/ThrottleWatch.Client/ThrottleWatch.Client.csproj src/ThrottleWatch.Client/
COPY src/ThrottleWatch.Dashboard/ThrottleWatch.Dashboard.csproj src/ThrottleWatch.Dashboard/
COPY samples/WebApiWithPolicies/WebApiWithPolicies.csproj samples/WebApiWithPolicies/

RUN dotnet restore samples/WebApiWithPolicies/WebApiWithPolicies.csproj

COPY src/ThrottleWatch.Client/ src/ThrottleWatch.Client/
COPY src/ThrottleWatch.Dashboard/ src/ThrottleWatch.Dashboard/
COPY samples/WebApiWithPolicies/ samples/WebApiWithPolicies/

RUN dotnet publish samples/WebApiWithPolicies/WebApiWithPolicies.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# curl is required by Compose healthchecks
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "WebApiWithPolicies.dll"]
