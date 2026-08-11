#!/usr/bin/env bash
# Pack ThrottleWatch.Client and smoke-install it into a temporary web app.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PROJECT="$ROOT/src/ThrottleWatch.Client/ThrottleWatch.Client.csproj"
OUT_DIR="${PACK_OUT:-$ROOT/artifacts/nuget}"
VERSION="${PACK_VERSION:-1.0.0}"
SMOKE_DIR="${PACK_SMOKE_DIR:-$ROOT/artifacts/pack-smoke}"

echo "==> Packing ThrottleWatch $VERSION → $OUT_DIR"
rm -rf "$OUT_DIR"
mkdir -p "$OUT_DIR"
dotnet pack "$PROJECT" \
  -c Release \
  -o "$OUT_DIR" \
  /p:Version="$VERSION" \
  --nologo

NUPKG="$OUT_DIR/ThrottleWatch.$VERSION.nupkg"
if [[ ! -f "$NUPKG" ]]; then
  echo "ERROR: expected package not found: $NUPKG" >&2
  ls -la "$OUT_DIR" >&2 || true
  exit 1
fi
echo "    OK $NUPKG"

echo "==> Smoke: create web app and add package from local feed"
rm -rf "$SMOKE_DIR"
mkdir -p "$SMOKE_DIR"
dotnet new web -n ThrottleWatchPackSmoke -o "$SMOKE_DIR" --force

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

dotnet add "$SMOKE_DIR/ThrottleWatchPackSmoke.csproj" package ThrottleWatch \
  --version "$VERSION" \
  --source "$OUT_DIR"
cat > "$SMOKE_DIR/Program.cs" <<'EOF'
using ThrottleWatch.Client.Configuration;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddThrottleWatch(options =>
{
    options.ApiBaseUrl = "http://localhost:5080";
    options.ApiKey = "dev-throttlewatch-key";
});

var app = builder.Build();
app.UseThrottleWatch();
app.MapGet("/", () => "ThrottleWatch pack smoke OK");
app.Run();
EOF

echo "==> Smoke: build consumer"
dotnet build "$SMOKE_DIR/ThrottleWatchPackSmoke.csproj" -c Release --nologo

echo ""
echo "Pack + smoke OK"
echo "  Package: $NUPKG"
echo "  Consumer: $SMOKE_DIR"
