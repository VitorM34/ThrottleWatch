# ThrottleWatch (Client SDK)

ASP.NET Core middleware + background sender that captures request metrics and posts them to **ThrottleWatch.Api**.

> This package is the **consumer SDK only**. The Dashboard UI is a separate package (`ThrottleWatch.Dashboard`). The API and PostgreSQL stack run separately (see the [repository](https://github.com/VitorM34/ThrottleWatch)).

## Install

```bash
dotnet add package ThrottleWatch
```

Local pack (before nuget.org publish):

```bash
dotnet pack src/ThrottleWatch.Client/ThrottleWatch.Client.csproj -c Release -o artifacts/nuget
dotnet add package ThrottleWatch --source ./artifacts/nuget --version 1.0.0
```

## Quick start

`Program.cs`:

```csharp
using ThrottleWatch.Client.Configuration;

builder.Services.AddThrottleWatch(builder.Configuration);
// ...
app.UseThrottleWatch();
```

`appsettings.json`:

```json
{
  "ThrottleWatch": {
    "ApiBaseUrl": "https://your-throttlewatch-api.example",
    "ApiKey": "your-shared-secret"
  }
}
```

`ApiKey` is sent as `X-ThrottleWatch-Key` on ingest calls. It must match a key configured on the API (one key = one tenant). The Client has no `TenantId` option — the Api stamps the tenant from the key. `/health` on the API stays public.

## Requirements

- .NET 10 / ASP.NET Core app
- A running **ThrottleWatch.Api** instance reachable at `ApiBaseUrl`

## Links

- Source: https://github.com/VitorM34/ThrottleWatch
- License: MIT
