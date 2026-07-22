COMPOSE ?= docker compose
API_PORT ?= 5080
DASHBOARD_PORT ?= 5100
HEALTH_RETRIES ?= 30
HEALTH_SLEEP ?= 2

.DEFAULT_GOAL := help

.PHONY: help up down db restart logs ps build health

help: ## Show available targets
	@awk 'BEGIN {FS = ":.*##"; printf "\nUsage:\n  make <target>\n\nTargets:\n"} /^[a-zA-Z_-]+:.*?##/ { printf "  %-12s %s\n", $$1, $$2 }' $(MAKEFILE_LIST)

up: ## Build and start Postgres + Api + Dashboard
	$(COMPOSE) up --build -d

down: ## Stop containers (keeps volume)
	$(COMPOSE) down

db: ## Start only PostgreSQL
	$(COMPOSE) up -d postgres

restart: ## Restart the stack
	$(COMPOSE) restart

logs: ## Tail container logs
	$(COMPOSE) logs -f --tail=100

ps: ## Show container status
	$(COMPOSE) ps

build: ## Build images without starting
	$(COMPOSE) build

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
