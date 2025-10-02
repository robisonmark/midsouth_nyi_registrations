.PHONY: help build test clean pack docker-build docker-up docker-down dev-up dev-down migrate

# Variables
VERSION ?= 1.0.0
PROJECT_NAME = RoBrosBaseDomainService
DOCKER_IMAGE = robros-journal-service

help: ## Display this help message
	@echo "Available targets:"
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "  \033[36m%-20s\033[0m %s\n", $$1, $$2}'

build: ## Build the project
	dotnet build $(PROJECT_NAME).csproj -c Release

test: ## Run tests
	dotnet test -c Release

clean: ## Clean build artifacts
	dotnet clean
	rm -rf bin/ obj/

restore: ## Restore NuGet packages
	dotnet restore

pack: ## Create NuGet package
	dotnet pack $(PROJECT_NAME).csproj -c Release -o ./nupkgs /p:Version=$(VERSION)

docker-build: ## Build Docker image
	docker build -t $(DOCKER_IMAGE):$(VERSION) -t $(DOCKER_IMAGE):latest .

docker-up: ## Start Docker Compose services
	docker-compose up -d

docker-down: ## Stop Docker Compose services
	docker-compose down

docker-logs: ## View Docker logs
	docker-compose logs -f

dev-up: ## Start development environment with hot reload
	docker-compose -f docker-compose.dev.yml up -d

dev-down: ## Stop development environment
	docker-compose -f docker-compose.dev.yml down

dev-logs: ## View development logs
	docker-compose -f docker-compose.dev.yml logs -f robros-service-dev

migrate: ## Run database migrations
	dotnet ef database update --project $(PROJECT_NAME).csproj

migration-add: ## Add new migration (use NAME=MigrationName)
	@if [ -z "$(NAME)" ]; then \
		echo "Error: Please specify NAME=MigrationName"; \
		exit 1; \
	fi
	dotnet ef migrations add $(NAME) --project $(PROJECT_NAME).csproj

db-reset: ## Reset database (WARNING: destroys data)
	docker-compose down -v
	docker-compose up -d

db-connect: ## Connect to PostgreSQL
	docker exec -it robros-journal-postgres psql -U robros_user -d robros_journal_db

format: ## Format code
	dotnet format

watch: ## Run with file watcher
	dotnet watch run

install-tools: ## Install required tools
	dotnet tool install --global dotnet-ef

version: ## Display version
	@echo "Current version: $(VERSION)"

bump-version: ## Bump version (use TYPE=major|minor|patch)
	@echo "Bumping version... (implement your version bump logic)"