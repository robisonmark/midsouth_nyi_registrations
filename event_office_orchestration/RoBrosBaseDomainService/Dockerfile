FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["RoBrosBaseDomainService.csproj", "./"]
RUN dotnet restore "RoBrosBaseDomainService.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "RoBrosBaseDomainService.csproj" -c Release -o /app/build

# Create NuGet package
FROM build AS publish
RUN dotnet pack "RoBrosBaseDomainService.csproj" -c Release -o /app/publish

# Runtime image for testing
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# For development/testing purposes
ENTRYPOINT ["tail", "-f", "/dev/null"]