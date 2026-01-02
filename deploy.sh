#!/bin/bash

# Colors
GREEN='\033[0;32m'
BLUE='\033[0;34m'
RED='\033[0;31m'
NC='\033[0m'

echo -e "${BLUE}MiniBank Kubernetes Deployment${NC}"
echo "================================"

# Variables
DOCKER_USERNAME="grahamkatana"
IMAGE_NAME="minibank-api"
VERSION="${1:-latest}"
K8S_NAMESPACE="minibank"

# Build Docker image
echo -e "\n${BLUE}Step 1: Building Docker image...${NC}"
docker build -t ${DOCKER_USERNAME}/${IMAGE_NAME}:${VERSION} .

if [ $? -ne 0 ]; then
    echo -e "${RED}Docker build failed!${NC}"
    exit 1
fi

echo -e "${GREEN}✓ Docker image built successfully${NC}"

# Tag as latest
echo -e "\n${BLUE}Step 2: Tagging image as latest...${NC}"
docker tag ${DOCKER_USERNAME}/${IMAGE_NAME}:${VERSION} ${DOCKER_USERNAME}/${IMAGE_NAME}:latest

# Push to Docker Hub
echo -e "\n${BLUE}Step 3: Pushing to Docker Hub...${NC}"
docker login

docker push ${DOCKER_USERNAME}/${IMAGE_NAME}:${VERSION}
docker push ${DOCKER_USERNAME}/${IMAGE_NAME}:latest

if [ $? -ne 0 ]; then
    echo -e "${RED}Docker push failed!${NC}"
    exit 1
fi

echo -e "${GREEN}✓ Image pushed to Docker Hub${NC}"

# Apply Kubernetes manifests
echo -e "\n${BLUE}Step 4: Applying Kubernetes manifests...${NC}"

kubectl apply -f k8s/00-namespace.yaml
echo -e "${GREEN}✓ Namespace created${NC}"

kubectl apply -f k8s/01-configmap.yaml
echo -e "${GREEN}✓ ConfigMap created${NC}"

kubectl apply -f k8s/02-secret.yaml
echo -e "${GREEN}✓ Secrets created${NC}"

kubectl apply -f k8s/03-sqlserver.yaml
echo -e "${GREEN}✓ SQL Server deployed${NC}"

# Wait for SQL Server to be ready
echo -e "\n${BLUE}Waiting for SQL Server to be ready...${NC}"
kubectl wait --for=condition=ready pod -l app=sqlserver -n ${K8S_NAMESPACE} --timeout=180s

# Run migration job
echo -e "\n${BLUE}Step 5: Running database migrations...${NC}"
kubectl delete job minibank-migration -n ${K8S_NAMESPACE} --ignore-not-found=true
kubectl apply -f k8s/06-migration-job.yaml

# Wait for migration to complete
kubectl wait --for=condition=complete job/minibank-migration -n ${K8S_NAMESPACE} --timeout=120s

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✓ Database migrations completed${NC}"
else
    echo -e "${RED}Migration failed! Check logs: kubectl logs -n ${K8S_NAMESPACE} job/minibank-migration${NC}"
fi

# Deploy API
echo -e "\n${BLUE}Step 6: Deploying API...${NC}"
kubectl apply -f k8s/04-api.yaml
echo -e "${GREEN}✓ API deployed${NC}"

# Wait for API to be ready
echo -e "\n${BLUE}Waiting for API pods to be ready...${NC}"
kubectl wait --for=condition=ready pod -l app=minibank-api -n ${K8S_NAMESPACE} --timeout=120s

# Get service URL
echo -e "\n${BLUE}Step 7: Getting service information...${NC}"
kubectl get svc -n ${K8S_NAMESPACE}

echo -e "\n${GREEN}========================================${NC}"
echo -e "${GREEN}✓ Deployment completed successfully!${NC}"
echo -e "${GREEN}========================================${NC}"

echo -e "\n${BLUE}Useful commands:${NC}"
echo "View pods:        kubectl get pods -n ${K8S_NAMESPACE}"
echo "View services:    kubectl get svc -n ${K8S_NAMESPACE}"
echo "View logs:        kubectl logs -f deployment/minibank-api -n ${K8S_NAMESPACE}"
echo "Scale API:        kubectl scale deployment minibank-api --replicas=3 -n ${K8S_NAMESPACE}"
echo "Delete all:       kubectl delete namespace ${K8S_NAMESPACE}"