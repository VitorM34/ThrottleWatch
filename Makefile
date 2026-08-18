COMPOSE ?= docker compose
API_PORT ?= 5080
DASHBOARD_PORT ?= 5100
SAMPLE_PORT ?= 5299
SAMPLE_URL ?= http://localhost:$(SAMPLE_PORT)
REQUESTS ?= 200
CONCURRENCY ?= 20
HEALTH_RETRIES ?= 30
HEALTH_SLEEP ?= 1

.DEFAULT_GOAL := help

.PHONY: help up up-rebuild down db restart logs ps build health sample sample-health demo-health load demo demo-rebuild pack-client pack-client-smoke pack-dashboard pack-dashboard-smoke

help: ## Show available targets
	@awk 'BEGIN {FS = ":.*##"; printf "\nUsage:\n  make <target>\n\nTargets:\n"} /^[a-zA-Z_-]+:.*?##/ { printf "  %-14s %s\n", $$1, $$2 }' $(MAKEFILE_LIST)

up: ## Start Postgres + Api + Dashboard (cached images, no rebuild)
	$(COMPOSE) up -d

up-rebuild: ## Rebuild images and start Postgres + Api + Dashboard
	$(COMPOSE) up --build -d

down: ## Stop containers (keeps volume; includes demo profile)
	$(COMPOSE) --profile demo down

db: ## Start only PostgreSQL
	$(COMPOSE) up -d postgres

restart: ## Restart the stack
	$(COMPOSE) restart

logs: ## Tail container logs
	$(COMPOSE) --profile demo logs -f --tail=100

ps: ## Show container status
	$(COMPOSE) --profile demo ps

build: ## Build images without starting (includes sample image)
	$(COMPOSE) --profile demo build

health: ## Wait for API /health (retries until ready)
	@echo "Waiting for API on http://localhost:$(API_PORT)/health ..."
	@i=0; \
	while [ $$i -lt $(HEALTH_RETRIES) ]; do \
		if curl -fsS "http://localhost:$(API_PORT)/health" >/dev/null 2>&1; then \
			curl -sS -w "\nHTTP %{http_code}\n" "http://localhost:$(API_PORT)/health"; \
			echo "Dashboard: http://localhost:$(DASHBOARD_PORT)"; \
			exit 0; \
		fi; \
		i=$$((i + 1)); \
		sleep $(HEALTH_SLEEP); \
	done; \
	echo "API did not become healthy in time."; \
	$(COMPOSE) ps; \
	$(COMPOSE) logs api --tail=40; \
	exit 1

sample-health: ## Wait for sample /health (Compose profile demo)
	@echo "Waiting for sample on $(SAMPLE_URL)/health ..."
	@i=0; \
	while [ $$i -lt $(HEALTH_RETRIES) ]; do \
		if curl -fsS "$(SAMPLE_URL)/health" >/dev/null 2>&1; then \
			curl -sS -w "\nHTTP %{http_code}\n" "$(SAMPLE_URL)/health"; \
			exit 0; \
		fi; \
		i=$$((i + 1)); \
		sleep $(HEALTH_SLEEP); \
	done; \
	echo "Sample did not become healthy in time."; \
	$(COMPOSE) --profile demo ps; \
	$(COMPOSE) logs sample --tail=40; \
	exit 1

demo-health: ## Wait for API + sample together (single poll loop)
	@echo "Waiting for API (:$(API_PORT)) and sample (:$(SAMPLE_PORT)) ..."
	@i=0; api_ok=0; sample_ok=0; \
	while [ $$i -lt $(HEALTH_RETRIES) ]; do \
		if [ $$api_ok -eq 0 ] && curl -fsS "http://localhost:$(API_PORT)/health" >/dev/null 2>&1; then \
			api_ok=1; echo "  API healthy"; \
		fi; \
		if [ $$sample_ok -eq 0 ] && curl -fsS "$(SAMPLE_URL)/health" >/dev/null 2>&1; then \
			sample_ok=1; echo "  Sample healthy"; \
		fi; \
		if [ $$api_ok -eq 1 ] && [ $$sample_ok -eq 1 ]; then \
			echo "Dashboard: http://localhost:$(DASHBOARD_PORT)"; \
			exit 0; \
		fi; \
		i=$$((i + 1)); \
		sleep $(HEALTH_SLEEP); \
	done; \
	echo "Demo services did not become healthy in time."; \
	$(COMPOSE) --profile demo ps; \
	$(COMPOSE) logs api --tail=20; \
	$(COMPOSE) logs sample --tail=20; \
	exit 1

sample: ## Run sample locally with dotnet (Api on :5287, or set ThrottleWatch__ApiBaseUrl)
	dotnet run --project samples/WebApiWithPolicies

load: ## Generate traffic against the sample (SAMPLE_URL / REQUESTS / CONCURRENCY)
	SAMPLE_URL="$(SAMPLE_URL)" REQUESTS="$(REQUESTS)" CONCURRENCY="$(CONCURRENCY)" ./scripts/load-sample.sh

demo: ## Fast demo: start stack+sample (no rebuild) + load
	$(COMPOSE) --profile demo up -d
	@$(MAKE) demo-health
	@$(MAKE) load
	@echo ""
	@echo "Demo ready"
	@echo "  Dashboard → http://localhost:$(DASHBOARD_PORT)"
	@echo "  Sample    → $(SAMPLE_URL)"
	@echo "  API       → http://localhost:$(API_PORT)"

demo-rebuild: ## Rebuild images, then run demo (use after code changes)
	$(COMPOSE) --profile demo up --build -d
	@$(MAKE) demo-health
	@$(MAKE) load
	@echo ""
	@echo "Demo ready (rebuilt)"
	@echo "  Dashboard → http://localhost:$(DASHBOARD_PORT)"
	@echo "  Sample    → $(SAMPLE_URL)"
	@echo "  API       → http://localhost:$(API_PORT)"

pack-client: ## Pack ThrottleWatch.Client → artifacts/nuget (PackageId ThrottleWatch)
	dotnet pack src/ThrottleWatch.Client/ThrottleWatch.Client.csproj -c Release -o artifacts/nuget --nologo
	@ls -la artifacts/nuget/ThrottleWatch*.nupkg

pack-client-smoke: ## Pack Client and smoke-install into a temporary web app
	./scripts/pack-client-smoke.sh

pack-dashboard: ## Pack ThrottleWatch.Dashboard → artifacts/nuget
	dotnet pack src/ThrottleWatch.Dashboard/ThrottleWatch.Dashboard.csproj -c Release -o artifacts/nuget --nologo
	@ls -la artifacts/nuget/ThrottleWatch.Dashboard*.nupkg

pack-dashboard-smoke: ## Pack Dashboard and smoke-install into a temporary web app
	./scripts/pack-dashboard-smoke.sh
