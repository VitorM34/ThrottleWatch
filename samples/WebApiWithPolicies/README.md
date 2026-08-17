# WebApiWithPolicies

Sample ASP.NET Core API with built-in rate limiting and the ThrottleWatch Client SDK.

Traffic is captured by `UseThrottleWatch` and flushed to `ThrottleWatch.Api` (`POST /api/metrics`) using the shared API key (`X-ThrottleWatch-Key` / `ThrottleWatch:ApiKey`).

## Fastest path (Docker)

From the repo root, with Docker running:

```bash
make demo
open http://localhost:5100
```

That starts Postgres + Api + Dashboard + this sample (cached images), waits for API + sample together, then runs `make load`.

Shared key defaults to `dev-throttlewatch-key` (`THROTTLEWATCH_API_KEY` in `.env.example`).

After code changes to the sample/Client/Api:

```bash
make demo-rebuild
```

## Local `dotnet run` (without sample container)

1. Postgres + Api (+ Dashboard) already up (`make up` or `dotnet run` on Api)
2. Point the Client at the Api host you are using (and keep `ApiKey` in sync):

```bash
# Api via Compose
ThrottleWatch__ApiBaseUrl=http://localhost:5080 make sample

# Api via local launchSettings (:5287)
make sample
```

3. Generate traffic:

```bash
make load
```

## Endpoints

| Path | Policy | Limit |
|------|--------|-------|
| `GET /health` | none | — |
| `GET /throttlewatch` | none | Embedded Dashboard (Overview) |
| `GET/POST /api/orders` | `strict` | 10 / 10s per API key (or IP) |
| `GET /api/products` | `standard` | 30 / 10s per API key (or IP) |

Compose wires `ThrottleWatch__ApiBaseUrl=http://api:8080` and the shared API key inside the `sample` service (profile `demo`).
