FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["RoBrosRegistrantsService/RoBrosRegistrantsService.csproj", "./"]
COPY ["RoBrosAddressesService/src/RoBrosAddressesService.csproj", "../RoBrosAddressesService/src/"]
RUN dotnet restore
COPY ./RoBrosRegistrantsService/ .
COPY ./RoBrosAddressesService/src/ ../RoBrosAddressesService/src/
WORKDIR "/src"

# Publish the application
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish --no-restore


FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RoBrosRegistrantsService.dll"]
