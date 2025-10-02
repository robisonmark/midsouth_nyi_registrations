FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files -- rearrange folder structure as needed or point these correctly
COPY ["samples/ConsumerApi/ConsumerApi.csproj", "./samples/ConsumerApi/"]
COPY ["src/RoBrosAddressesService.csproj", "/src/"]

# Restore dependencies
RUN dotnet restore "samples/ConsumerApi/ConsumerApi.csproj"

# Copy everything else
COPY . .

# Build the sample app
WORKDIR "/src/samples/ConsumerApi"
RUN dotnet build "ConsumerApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ConsumerApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ConsumerApi.dll"]