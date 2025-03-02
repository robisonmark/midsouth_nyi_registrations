# Use the official .NET Core SDK image to build the app
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base

# Set the working directory
WORKDIR /app

# Copy the project file and restore dependencies
COPY *.csproj .
RUN dotnet restore

# Copy the rest of the application code
COPY . .

# Build the application
RUN dotnet publish -c Release -o out

# Use the official ASP.NET Core runtime image for the runtime environment
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

# Set the working directory
WORKDIR /app

# Copy the build output from the build stage
COPY --from=base /app/out .

# Expose the port the app runs on
EXPOSE 80

# Define the entry point for the application
ENTRYPOINT ["dotnet", "RoBrosRegistrantsService.dll"]
