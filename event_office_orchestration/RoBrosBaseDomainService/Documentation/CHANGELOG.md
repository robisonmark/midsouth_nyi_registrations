# Changelog

All notable changes to the RoBrosBaseDomainService will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-09-30

### Added
- Initial release of RoBrosBaseDomainService
- Core journal/audit functionality with entity tracking
- Background processing using Channels for async operations
- PostgreSQL support with JSONB storage for entity payloads
- Comprehensive audit trail with created_by, created_at, updated_by, updated_at
- Version tracking for entity history
- `IJournalService` for synchronous operations
- `IAsyncJournalService` for background async operations
- Service collection extensions for easy integration
- Database migration support
- Docker and Docker Compose configuration
- Hot reload support for development
- Unit tests with xUnit, Moq, and FluentAssertions
- Comprehensive documentation and examples
- GitHub Actions CI/CD pipeline
- Makefile for common development tasks

### Features
- Self-contained library that integrates with any .NET 9.0 service
- Uses calling service's database connection
- Thread-safe async processing with bounded channels
- Automatic database schema creation and migrations
- Optimized indexes for common query patterns
- Retry logic for transient database failures
- Configurable channel capacity for high-load scenarios

### Developer Experience
- Hot reload during development
- Example integration code
- Quick start guide
- Comprehensive README
- Docker development environment
- PostgreSQL included for testing
- Make commands for common tasks

## [Unreleased]

### Planned
- Support for additional database providers (SQL Server, MySQL)
- Batch processing capabilities
- Enhanced query filters and search
- Performance metrics and monitoring
- Archive/cleanup strategies for old journal entries
- Event notifications on entity changes
- GraphQL support
- gRPC support