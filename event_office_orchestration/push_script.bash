#!/bin/bash

# Variables
RESOURCE_GROUP="your-existing-rg"
LOCATION="eastus"
REGISTRY_NAME="youruniqueregistry"
ENV_NAME="event-office-env"

# Create ACR if needed
az acr create \
  --resource-group $RESOURCE_GROUP \
  --name $REGISTRY_NAME \
  --sku Basic \
  --admin-enabled true

# Login to ACR
az acr login --name $REGISTRY_NAME

# Build and push Events Service
cd /path/to/RoBrosEventsService
docker build -t $REGISTRY_NAME.azurecr.io/events-service:latest .
docker push $REGISTRY_NAME.azurecr.io/events-service:latest

# Build and push Registrants Service
cd /path/to/RoBrosRegistrantsService
docker build -t $REGISTRY_NAME.azurecr.io/registrants-service:latest .
docker push $REGISTRY_NAME.azurecr.io/registrants-service:latest

# Create Container Apps Environment
az containerapp env create \
  --name $ENV_NAME \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION

# Get ACR credentials
ACR_USERNAME=$REGISTRY_NAME
ACR_PASSWORD=$(az acr credential show --name $REGISTRY_NAME --query "passwords[0].value" -o tsv)

# Deploy Events Service
az containerapp create \
  --name events-service \
  --resource-group $RESOURCE_GROUP \
  --environment $ENV_NAME \
  --image $REGISTRY_NAME.azurecr.io/events-service:latest \
  --registry-server $REGISTRY_NAME.azurecr.io \
  --registry-username $ACR_USERNAME \
  --registry-password $ACR_PASSWORD \
  --target-port 8080 \
  --ingress external \
  --min-replicas 0 \
  --max-replicas 5 \
  --env-vars "ASPNETCORE_URLS=http://+:8080"

# Deploy Registrants Service
az containerapp create \
  --name registrants-service \
  --resource-group $RESOURCE_GROUP \
  --environment $ENV_NAME \
  --image $REGISTRY_NAME.azurecr.io/registrants-service:latest \
  --registry-server $REGISTRY_NAME.azurecr.io \
  --registry-username $ACR_USERNAME \
  --registry-password $ACR_PASSWORD \
  --target-port 8080 \
  --ingress external \
  --min-replicas 0 \
  --max-replicas 5 \
  --env-vars "ASPNETCORE_URLS=http://+:8080"

echo "Deployment complete!"
az containerapp list --resource-group $RESOURCE_GROUP -o table