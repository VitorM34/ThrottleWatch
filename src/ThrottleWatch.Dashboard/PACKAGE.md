# ThrottleWatch.Dashboard

Embeddable Blazor Dashboard (Razor Class Library) for **ThrottleWatch**. Maps the UI at `/throttlewatch` (configurable).

> This package is the **UI only**. Metrics collection is the separate [`ThrottleWatch`](https://www.nuget.org/packages/ThrottleWatch) Client SDK. The API and PostgreSQL stack still run separately (see the [repository](https://github.com/VitorM34/ThrottleWatch)).

## Install

```bash
dotnet add package ThrottleWatch
dotnet add package ThrottleWatch.Dashboard
```

Local pack (before nuget.org publish):

```bash
make pack-dashboard
dotnet add package ThrottleWatch.Dashboard --source ./artifacts/nuget --version 1.0.0
```

## Quick start

`Program.cs`:

```csharp
using ThrottleWatch.Client.Configuration;
using ThrottleWatch.Dashboard.Extensions;

builder.Services.AddThrottleWatch(builder.Configuration);
builder.Services.AddThrottleWatchDashboard(builder.Configuration);

var app = builder.Build();
app.UseThrottleWatch();
app.UseThrottleWatchDashboard(); // default: /throttlewatch
```

`GET /health` on your API stays yours. Dashboard health is `GET /throttlewatch/health`.

Standalone Compose Dashboard remains at the host root (`UseThrottleWatchDashboard("/")`).

`appsettings.json` (Dashboard reads the same `ThrottleWatch` section for the API URL and key):

```json
{
  "ThrottleWatch": {
    "ApiBaseUrl": "https://your-throttlewatch-api.example",
    "ApiKey": "your-shared-secret"
  }
}
```

## Requirements

- .NET 10 / ASP.NET Core app (Blazor Interactive Server)
- A running **ThrottleWatch.Api** instance reachable at `ApiBaseUrl`

## Links

- Source: https://github.com/VitorM34/ThrottleWatch
- License: MIT
