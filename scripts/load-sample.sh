#!/usr/bin/env bash
set -euo pipefail

SAMPLE_URL="${SAMPLE_URL:-http://localhost:5299}"
REQUESTS="${REQUESTS:-200}"
CONCURRENCY="${CONCURRENCY:-20}"
KEYS_CSV="${KEYS_CSV:-demo-key-a,demo-key-b,demo-key-c,burst-key}"
PATHS_CSV="${PATHS_CSV:-/api/orders,/api/orders,/api/orders,/api/products,/api/products}"

echo "Load → ${SAMPLE_URL} (${REQUESTS} requests, concurrency ${CONCURRENCY})"

if ! curl -fsS "${SAMPLE_URL}/health" >/dev/null; then
  echo "Sample not reachable at ${SAMPLE_URL}. Start it with:"
  echo "  make sample"
  echo "  # or: dotnet run --project samples/WebApiWithPolicies"
  exit 1
fi

tmp_dir="$(mktemp -d)"
trap 'rm -rf "${tmp_dir}"' EXIT

worker() {
  local i="$1"
  IFS=',' read -r -a keys <<< "${KEYS_CSV}"
  IFS=',' read -r -a paths <<< "${PATHS_CSV}"
  local key="${keys[$((i % ${#keys[@]}))]}"
  local path="${paths[$((i % ${#paths[@]}))]}"
  curl -sS -o /dev/null -w '%{http_code}\n' \
    -H "X-Api-Key: ${key}" \
    -H "Accept: application/json" \
    "${SAMPLE_URL}${path}" || echo "000"
}

export -f worker
export SAMPLE_URL KEYS_CSV PATHS_CSV

seq 1 "${REQUESTS}" | xargs -P "${CONCURRENCY}" -I{} bash -c 'worker "$@"' _ {} \
  > "${tmp_dir}/codes.txt"

ok=0
blocked=0
other=0

while read -r code; do
  case "${code}" in
    200|201|202) ok=$((ok + 1)) ;;
    429) blocked=$((blocked + 1)) ;;
    *) other=$((other + 1)) ;;
  esac
done < "${tmp_dir}/codes.txt"

echo "Done: ok=${ok} blocked(429)=${blocked} other=${other}"
echo "Check Dashboard overview / API summary — metrics flush within ~1s."
