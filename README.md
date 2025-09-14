"# BankingSolution" 
# BankingSolution

[![.NET 8](https://img.shields.io/badge/.NET-8.0-blueviolet?logo=dotnet)](https://dotnet.microsoft.com/)  
[![MongoDB](https://img.shields.io/badge/Database-MongoDB-brightgreen?logo=mongodb)](https://www.mongodb.com/)  
[![RabbitMQ](https://img.shields.io/badge/Messaging-RabbitMQ-orange?logo=rabbitmq)](https://www.rabbitmq.com/)  
[![Build](https://img.shields.io/badge/Build-Passing-brightgreen)](#)  
[![License](https://img.shields.io/badge/License-MIT-lightgrey)](LICENSE)  

BankingSolution is a **.NET 8 microservices-based banking system** that demonstrates **event-driven architecture** using **RabbitMQ** and **MongoDB**.  
It allows creating customers, opening accounts, recording transactions, and viewing customer summaries through **CQRS-style projections**. *(Mini-Banking application)*  

## Features
Customer Service (Customers.Api)
   Create and manage customers
   View customer summaries (accounts, balances, transactions) via projections

Transaction Service (Transactions.Api)
   Open accounts for customers
   Record transactions (credit/debit)
   Update account balances

Event-Driven Messaging
   Decoupled services communicating via RabbitMQ
   Events published/consumed across services:
      CustomerCreatedEvent
      AccountCreatedEvent
      TransactionCreatedEvent
      AccountBalanceUpdatedEvent

## Requirements
- .NET 8 SDK
- MongoDB (for data storage)
- RabbitMQ (for message/event handling)

## configurations
Set appsettings.json:

"Mongo": {
  "ConnectionString": "mongodb://localhost:27017",
  "DatabaseName": "BankingDb"
},
"RabbitMq": {
  "ConnectionString": "amqp://guest:guest@localhost:5672/",
  "Exchange": "domain.events"
}


## Setup Instructions

1. Clone the repository:

  git clone https://github.com/RouaaAssaf/BankingSolution.git
  cd BankingSolution


2.Restore NuGet packages:

  dotnet restore
 
3.Start RabbitMQ and MongoDB locally.

4.Run the services:

  cd src/Transactions.Api
  dotnet run

  cd src/Customers.Api
  dotnet run



APIs will be available at:
  Transactions: http://localhost:5000
  Customers: http://localhost:5001

//API Endpoints:

1.Customers.Api

Method	  Endpoint	                             Description
POST    /api/Customers                         Create a new customer
GET	  /api/customers/{id}/summary	           Get customer summary (accounts + balances)


2.Transactions.Api

 
Method	  Endpoint	                              Description
POST    /api/accounts                          Open a new account for a customer
POST   /api/accounts/{accountId}/transactions  Add a transaction (credit/debit)


//All unit tests are in the tests/Banking.UnitTests folder:

    dotnet test

//Example Requests
    1. Create Customer
           POST /api/customers
           Content-Type: application/json

             {
              "firstName": "John",
              "lastName": "Doe",
              "email": "john.doe@email.com"
             }
    ➡️ Publishes CustomerCreatedEvent → Transactions.Api automatically opens an account for the customer.


    2. Add Transaction
          POST /api/accounts/{accountId}/transactions
          Content-Type: application/json

             {
              "amount": 250,
              "type": "Credit",
              "description": "Salary deposit"
             }

    ➡️ Publishes TransactionCreatedEvent + AccountBalanceUpdatedEvent.
       Customers.Api consumes the balance update and refreshes the customer’s summary.


    3. Get Customer Summary
         GET /api/customers/{customerId}/summary

  
//Event Flow Diagram (Optional)
    sequenceDiagram
       participant C as Customers.Api
       participant T as Transactions.Api
       participant MQ as RabbitMQ
       C->>MQ: CustomerCreatedEvent
       MQ->>T: CustomerCreatedEvent
       T->>MQ: AccountCreatedEvent
       MQ->>C: AccountCreatedEvent
       T->>MQ: TransactionCreatedEvent
       T->>MQ: AccountBalanceUpdatedEvent
       MQ->>C: AccountBalanceUpdatedEvent

// Notes

   ✅ The API now uses MongoDB for persistence (SQLite removed).

   ✅ Cross-service communication happens via RabbitMQ events (not API calls).

   ✅ Projections in Customers.Api are kept up to date by consuming AccountCreatedEvent and AccountBalanceUpdatedEvent.

   ✅ No need to run dotnet ef database update anymore — there are no EF Core migrations in this version.