# Banking Microservices

A microservices-based banking application built with .NET 8, demonstrating modern distributed systems architecture.

## Architecture Overview

This application is built using a microservices architecture with the following components:

1. **AccountService**: Manages customers and accounts
   - Features: Customer CRUD, Account creation/management, deposits/withdrawals
   - Port: 5259

2. **TransactionService**: Handles money transfers
   - Features: Transaction processing between accounts
   - Port: 5124

3. **NotificationService**: Manages customer notifications
   - Features: Email notifications for account activities
   - Port: 5240

4. **ApiGateway**: Ocelot-based API Gateway
   - Routes all requests to appropriate microservices
   - Provides a unified entry point for all services
   - Port: 5209

### System Components Diagram
┌─────────────┐
│   Client    │
└──────┬──────┘
│
▼
┌──────────────┐
│  API Gateway │
│   (Ocelot)   │
└───┬─────┬────┘
│     │
▼     │     ▼
┌─────────┐ ┌──────────┐ ┌──────────────┐
│ Account │ │Transaction│ │ Notification │
│ Service │ │ Service   │ │   Service    │
└─────────┘ └──────────┘ └──────────────┘
│           │               │
▼           ▼               ▼
┌─────────┐ ┌──────────┐ ┌──────────────┐
│ Account │ │Transaction│ │ Notification │
│   DB    │ │    DB     │ │      DB      │
└─────────┘ └──────────┘ └──────────────┘


## API Documentation

### Account Service

#### Customers

- **GET /api/customers** - Get all customers
- **GET /api/customers/{id}** - Get customer by ID
- **POST /api/customers** - Create a new customer
  {
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@example.com",
    "phoneNumber": "123-456-7890"
  }

- **PUT /api/customers/{id}** - Update a customer
- **DELETE /api/customers/{id}** - Delete a customer

#### Accounts

- **GET /api/accounts** - Get all accounts
- **GET /api/accounts/{id}** - Get account by ID
- **POST /api/accounts** - Create a new account
{
  "customerId": 1,
  "accountType": "Checking",
  "balance": 1000.00
}

- **PUT /api/accounts/{id}/deposit** - Deposit money
{
  "amount": 500.00
}

- **PUT /api/accounts/{id}/withdraw** - Withdraw money
{
  "amount": 200.00
}


### Transaction Service

- **GET /api/transactions** - Get all transactions
- **GET /api/transactions/{id}** - Get transaction by ID
- **POST /api/transactions** - Create a new transaction
{
  "fromAccountId": 1,
  "toAccountId": 2,
  "amount": 100.00,
  "description": "Monthly rent payment"
}


### Notification Service

- **GET /api/notifications** - Get all notifications
- **GET /api/notifications/{id}** - Get notification by ID
- **GET /api/notifications/customer/{customerId}** - Get notifications for a customer


### Getting Started
#### Prerequisites

- **.NET 8.0 SDK**
- **Docker and Docker Compose (optional)**

#### Running Locally

1. Clone the repository
2. Run the startup script:

./start-services.sh

3. Access the API Gateway at http://localhost:5209


### Technologies Used

.NET 8.0
Entity Framework Core
SQLite (for development)
Ocelot API Gateway
MassTransit with RabbitMQ (for service-to-service communication)
Docker & Docker Compose
Swagger for API documentation