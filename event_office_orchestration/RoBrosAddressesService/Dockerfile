FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files -- rearrange folder structure as needed or point these correctly
COPY ["src/samples/AddressService.Sample/AddressService.Sample.csproj", "src/samples/AddressService.Sample/"]
COPY ["src/RoBrosAddressesService.csproj", "src/"]

# Restore dependencies
RUN dotnet restore "src/samples/AddressService.Sample/AddressService.Sample.csproj"

# Copy everything else
COPY . .

# Build the sample app
WORKDIR "/src/samples/AddressService.Sample"
RUN dotnet build "AddressService.Sample.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AddressService.Sample.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AddressService.Sample.dll"]