"# BankingSolution" 
# BankingSolution

BankingSolution is a .NET Core API that allows opening accounts for existing customers, adding transactions, and viewing customer summaries.

## Features
- Open accounts for existing customers
- Add transactions (credit/debit)
- View customer summary with balance and transactions
- Clean architecture with repositories, services, and controllers

## Requirements
- .NET 7 SDK
- SQLite (EF Core included)


## Setup Instructions

1. Clone the repository:
```bash
git clone https://github.com/RouaaAssaf/BankingSolution.git
cd BankingSolution/src/Banking.WebApi

2.Restore NuGet packages:

dotnet restore


3.Apply migrations and seed the database:

dotnet ef database update


4.Run the API:

dotnet run

//The API will be available at:

    HTTP: http://localhost:5000
    HTTPS: https://localhost:5001

//API Endpoints:

Method	  Endpoint	                             Description
POST    /api/accounts/open	                    Open a new account
POST   /api/accounts/{accountId}/transaction	Add a transaction to an account
GET	  /api/customers/{id}/summary	            Get customer summary with balance and transactions

//Example: Open Account
   POST /api/accounts/open
      {
        "customerId": "aaa",
        "initialCredit": 100
      }

//Example: Get Customer Summary
    GET /api/customers/aaa/summary

//Running Unit Tests

    All unit tests are in the tests/Banking.UnitTests folder.
    Run tests with:

    dotnet test

// Notes

   The API uses SQLite for simplicity.
   Accounts and Transactions are handled through separate services.
   Make sure to run dotnet ef database update before using the API to initialize the database.   
