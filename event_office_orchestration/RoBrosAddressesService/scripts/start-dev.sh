#!/bin/bash

echo "🚀 Starting Address Service Development Environment..."

# Build and start containers
docker-compose -f docker-compose.yml up -d

# Wait for SQL Server to be ready
echo "⏳ Waiting for SQL Server to be ready..."
sleep 30

# Run database initialization if needed
echo "🗄️ Initializing database..."
docker-compose -f docker/docker-compose.yml exec sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Password123 -Q "CREATE DATABASE AddressServiceTest"

echo "✅ Development environment ready!"
echo "📊 Swagger UI: http://localhost:5000/swagger"
echo "🗄️ SQL Server: localhost:1434 (sa/YourStrong@Password123)"