#!/bin/bash

# Banking Microservices Startup Script
echo "Starting Banking Microservices..."

# Define service directories and output files
ACCOUNT_DIR="./AccountService"
TRANSACTION_DIR="./TransactionService"
NOTIFICATION_DIR="./NotificationService"
GATEWAY_DIR="./ApiGateway"

LOG_DIR="./logs"
mkdir -p $LOG_DIR

# Function to start a service
start_service() {
    local service_dir=$1
    local service_name=$2
    local log_file="$LOG_DIR/${service_name}.log"
    
    echo "Starting $service_name..."
    cd $service_dir
    dotnet build > /dev/null 2>&1
    nohup dotnet bin/Debug/net8.0/${service_name}.dll > $log_file 2>&1 &
    cd ..
    echo "$service_name started with PID $! - Logs: $log_file"
}

# Start all services
start_service $ACCOUNT_DIR "AccountService"
sleep 2
start_service $TRANSACTION_DIR "TransactionService"
sleep 2
start_service $NOTIFICATION_DIR "NotificationService"
sleep 2
start_service $GATEWAY_DIR "ApiGateway"

echo "All services started. Check the logs directory for output."
echo "API Gateway is accessible at http://localhost:5209"