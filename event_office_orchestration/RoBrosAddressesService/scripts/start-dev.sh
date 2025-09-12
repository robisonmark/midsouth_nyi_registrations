#!/bin/bash

echo "🚀 Starting Address Service Development Environment..."

# Build and start containers
docker-compose -f docker-compose.yml up -d

# Wait for SQL Server to be ready
echo "⏳ Waiting for PostGres SQL Server to be ready..."
sleep 30

# Run database initialization if needed
echo "🗄️ Initializing database..."
docker-compose -f docker-compose.yml exec db /pgdata -S localhost -U postgres -P YourPassword -Q "CREATE DATABASE AddressServiceTest"

echo "✅ Development environment ready!"
echo "📊 Swagger UI: http://localhost:5000/swagger"
echo "🗄️ Postgres Server: localhost:5432 (sa/YourStrong@Password123)"