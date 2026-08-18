#!/usr/bin/env bash
# Pack ThrottleWatch.Dashboard and smoke-install it into a temporary web app.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
DASHBOARD_PROJECT="$ROOT/src/ThrottleWatch.Dashboard/ThrottleWatch.Dashboard.csproj"
CLIENT_PROJECT="$ROOT/src/ThrottleWatch.Client/ThrottleWatch.Client.csproj"
OUT_DIR="${PACK_OUT:-$ROOT/artifacts/nuget}"
VERSION="${PACK_VERSION:-1.0.0}"
SMOKE_DIR="${PACK_SMOKE_DIR:-$ROOT/artifacts/pack-dashboard-smoke}"

echo "==> Packing ThrottleWatch.Dashboard $VERSION → $OUT_DIR"
rm -rf "$OUT_DIR"
mkdir -p "$OUT_DIR"
dotnet pack "$DASHBOARD_PROJECT" \
  -c Release \
  -o "$OUT_DIR" \
  /p:Version="$VERSION" \
  --nologo

NUPKG="$OUT_DIR/ThrottleWatch.Dashboard.$VERSION.nupkg"
if [[ ! -f "$NUPKG" ]]; then
  echo "ERROR: expected package not found: $NUPKG" >&2
  ls -la "$OUT_DIR" >&2 || true
  exit 1
fi
echo "    OK $NUPKG"

echo "==> Assert static web assets and no Client UI leak"
LISTING="$(unzip -Z1 "$NUPKG")"
echo "$LISTING" | grep -q 'ThrottleWatch.Dashboard.dll' \
  || { echo "ERROR: nupkg missing ThrottleWatch.Dashboard.dll" >&2; exit 1; }
echo "$LISTING" | grep -Eq 'app\.css|staticwebassets|wwwroot' \
  || { echo "ERROR: nupkg missing Dashboard static assets" >&2; echo "$LISTING" >&2; exit 1; }
echo "$LISTING" | grep -q 'appsettings' \
  && { echo "ERROR: nupkg must not include leftover appsettings" >&2; echo "$LISTING" >&2; exit 1; }

echo "==> Pack Client and assert it has no Blazor/ApexCharts dependency"
dotnet pack "$CLIENT_PROJECT" \
  -c Release \
  -o "$OUT_DIR" \
  /p:Version="$VERSION" \
  --nologo

CLIENT_NUSPEC="$(unzip -p "$OUT_DIR/ThrottleWatch.$VERSION.nupkg" '*.nuspec')"
if echo "$CLIENT_NUSPEC" | grep -Ei 'ApexCharts|ThrottleWatch\.Dashboard|Microsoft\.AspNetCore\.Components'; then
  echo "ERROR: Client package must not depend on Dashboard/Blazor/ApexCharts" >&2
  echo "$CLIENT_NUSPEC" >&2
  exit 1
fi
echo "    OK Client nuspec has no Dashboard/Blazor/ApexCharts dependency"

echo "==> Smoke: create web app and add package from local feed"
rm -rf "$SMOKE_DIR"
mkdir -p "$SMOKE_DIR"
dotnet new web -n ThrottleWatchDashboardPackSmoke -o "$SMOKE_DIR" --force

cat > "$SMOKE_DIR/ThrottleWatchDashboardPackSmoke.csproj" <<EOF
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <RequiresAspNetWebAssets>true</RequiresAspNetWebAssets>
  </PropertyGroup>

</Project>
EOF

cat > "$SMOKE_DIR/nuget.config" <<EOF
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="local-throttlewatch" value="$OUT_DIR" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
EOF

# nuget.config already lists local feed + nuget.org. Do not pass --source:
# that would hide nuget.org and SDK packs (Blazor-ApexCharts, AspNetCore assets).
dotnet add "$SMOKE_DIR/ThrottleWatchDashboardPackSmoke.csproj" package ThrottleWatch.Dashboard \
  --version "$VERSION"

cat > "$SMOKE_DIR/appsettings.json" <<'EOF'
{
  "ThrottleWatch": {
    "ApiBaseUrl": "http://localhost:5080",
    "ApiKey": "dev-throttlewatch-key"
  }
}
EOF

cat > "$SMOKE_DIR/Program.cs" <<'EOF'
using ThrottleWatch.Dashboard.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddThrottleWatchDashboard(builder.Configuration);

var app = builder.Build();
app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));
app.UseThrottleWatchDashboard();
app.Run();
EOF

echo "==> Smoke: build consumer (no ProjectReference to src/)"
if grep -q "ProjectReference" "$SMOKE_DIR/ThrottleWatchDashboardPackSmoke.csproj"; then
  echo "ERROR: smoke consumer must not use ProjectReference" >&2
  cat "$SMOKE_DIR/ThrottleWatchDashboardPackSmoke.csproj" >&2
  exit 1
fi

dotnet build "$SMOKE_DIR/ThrottleWatchDashboardPackSmoke.csproj" -c Release --nologo

echo ""
echo "Pack + smoke OK"
echo "  Package: $NUPKG"
echo "  Consumer: $SMOKE_DIR"
