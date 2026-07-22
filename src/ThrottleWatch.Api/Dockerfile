# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY global.json Directory.Build.props ThrottleWatch.slnx ./
COPY src/ThrottleWatch.Domain/ThrottleWatch.Domain.csproj src/ThrottleWatch.Domain/
COPY src/ThrottleWatch.Application/ThrottleWatch.Application.csproj src/ThrottleWatch.Application/
COPY src/ThrottleWatch.Infrastructure/ThrottleWatch.Infrastructure.csproj src/ThrottleWatch.Infrastructure/
COPY src/ThrottleWatch.Api/ThrottleWatch.Api.csproj src/ThrottleWatch.Api/

RUN dotnet restore src/ThrottleWatch.Api/ThrottleWatch.Api.csproj

COPY src/ThrottleWatch.Domain/ src/ThrottleWatch.Domain/
COPY src/ThrottleWatch.Application/ src/ThrottleWatch.Application/
COPY src/ThrottleWatch.Infrastructure/ src/ThrottleWatch.Infrastructure/
COPY src/ThrottleWatch.Api/ src/ThrottleWatch.Api/

RUN dotnet publish src/ThrottleWatch.Api/ThrottleWatch.Api.csproj \
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

ENTRYPOINT ["dotnet", "ThrottleWatch.Api.dll"]
