# Variables
PACKAGE_VERSION ?= 1.0.0
DOCKER_REGISTRY ?= your-registry.com
PROJECT_NAME = AddressPackage

# Default target
.PHONY: help
help:
	@echo "Available targets:"
	@echo "  build          - Build the package"
	@echo "  test           - Run all tests"
	@echo "  test-unit      - Run unit tests only"
	@echo "  test-integration - Run integration tests only"
	@echo "  pack           - Create NuGet package"
	@echo "  publish        - Publish package to NuGet"
	@echo "  docker-up      - Start development environment"
	@echo "  docker-down    - Stop development environment"
	@echo "  docker-build   - Build consumer API docker image"
	@echo "  clean          - Clean build artifacts"
	@echo "  migrate        - Run database migrations"

# Build targets
.PHONY: build
build:
	@echo "Building AddressPackage..."
	dotnet build AddressPackage.csproj --configuration Release

.PHONY: clean
clean:
	@echo "Cleaning build artifacts..."
	dotnet clean AddressPackage.csproj
	dotnet clean AddressPackage.Tests/AddressPackage.Tests.csproj
	dotnet clean samples/ConsumerApi/ConsumerApi.csproj
	rm -rf bin obj
	rm -rf AddressPackage.Tests/bin AddressPackage.Tests/obj
	rm -rf samples/ConsumerApi/bin samples/ConsumerApi/obj

# Test targets
.PHONY: test
test: test-unit test-integration

.PHONY: test-unit
test-unit:
	@echo "Running unit tests..."
	dotnet test AddressPackage.Tests/AddressPackage.Tests.csproj --configuration Release --filter Category!=Integration --logger trx --results-directory ./test-results

.PHONY: test-integration
test-integration: docker-up
	@echo "Running integration tests..."
	dotnet test AddressPackage.Tests/AddressPackage.Tests.csproj --configuration Release --filter Category=Integration --logger trx --results-directory ./test-results

# Package targets
.PHONY: pack
pack: build
	@echo "Creating NuGet package..."
	dotnet pack AddressPackage.csproj --configuration Release --output ./packages --no-build -p:PackageVersion=$(PACKAGE_VERSION)

.PHONY: publish
publish: pack
	@echo "Publishing package to NuGet..."
	dotnet nuget push ./packages/AddressPackage.$(PACKAGE_VERSION).nupkg --source https://api.nuget.org/v3/index.json

# Docker targets
.PHONY: docker-up
docker-up:
	@echo "Starting development environment..."
	docker-compose up -d postgres postgres_test
	@echo "Waiting for PostgreSQL to be ready..."
	@sleep 5
	@docker-compose exec -T postgres pg_isready -U devuser -d addressdev || (echo "Waiting for postgres..." && sleep 10)

.PHONY: docker-down
docker-down:
	@echo "Stopping development environment..."
	docker-compose down

.PHONY: docker-build
docker-build:
	@echo "Building consumer API docker image..."
	docker build -f samples/ConsumerApi/Dockerfile -t $(DOCKER_REGISTRY)/consumer-api:$(PACKAGE_VERSION) .

.PHONY: docker-push
docker-push: docker-build
	@echo "Pushing consumer API docker image..."
	docker push $(DOCKER_REGISTRY)/consumer-api:$(PACKAGE_VERSION)

# Database targets
.PHONY: migrate
migrate: docker-up
	@echo "Running database migrations..."
	@docker-compose exec -T postgres psql -U devuser -d addressdev -f /docker-entrypoint-initdb.d/init.sql

# Development targets
.PHONY: dev
dev: docker-up
	@echo "Starting development environment..."
	dotnet watch --project samples/ConsumerApi/ConsumerApi.csproj

.PHONY: dev-build
dev-build: docker-up
	@echo "Building and running consumer API..."
	docker-compose up --build consumer-api

# Versioning
.PHONY: version-patch
version-patch:
	@echo "Incrementing patch version..."
	$(eval NEW_VERSION := $(shell echo $(PACKAGE_VERSION) | awk -F. '{$$3++; print $$1"."$$2"."$$3}'))
	@echo "New version: $(NEW_VERSION)"
	@$(MAKE) pack PACKAGE_VERSION=$(NEW_VERSION)

.PHONY: version-minor
version-minor:
	@echo "Incrementing minor version..."
	$(eval NEW_VERSION := $(shell echo $(PACKAGE_VERSION) | awk -F. '{$$2++; $$3=0; print $$1"."$$2"."$$3}'))
	@echo "New version: $(NEW_VERSION)"
	@$(MAKE) pack PACKAGE_VERSION=$(NEW_VERSION)

.PHONY: version-major
version-major:
	@echo "Incrementing major version..."
	$(eval NEW_VERSION := $(shell echo $(PACKAGE_VERSION) | awk -F. '{$$1++; $$2=0; $$3=0; print $$1"."$$2"."$$3}'))
	@echo "New version: $(NEW_VERSION)"
	@$(MAKE) pack PACKAGE_VERSION=$(NEW_VERSION)

# Quality checks
.PHONY: lint
lint:
	@echo "Running code analysis..."
	dotnet format --verify-no-changes --verbosity diagnostic
	dotnet build --configuration Release --verbosity quiet /p:TreatWarningsAsErrors=true

.PHONY: security
security:
	@echo "Running security audit..."
	dotnet list package --vulnerable --include-transitive

# CI/CD helpers
.PHONY: ci-build
ci-build: clean build test pack

.PHONY: ci-deploy
ci-deploy: ci-build publish docker-push