# Restaurant API

## Description
Restaurant API is a .NET application designed to manage restaurants and their dishes. The application uses JWT tokens for user authentication and implements security policies using FluentValidation. This project is developed based on the course [Practical ASP.NET Core REST Web API from Scratch (C#)](https://www.udemy.com/course/praktyczny-kurs-aspnet-core-rest-web-api-od-podstaw/).

## Requirements
- [.NET 8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (local or Azure)

## Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/DreKurdeK/RestaurantAPI
   cd RestaurantAPI
2. Install dependencies:

    ```bash
    dotnet restore

3. Configure the database connection in the appsettings.json file:

    ```json
    {
    "ConnectionStrings": {
        "RestaurantDbConnection": "your_connection_string"
    },
    "Authentication": {
        "JwtKey": "your_jwt_key",
        "JwtIssuer": "your_jwt_issuer"
    },
    "AllowedOrigins": "http://your-frontend-origin"
    }

4. Run migrations to create the database:

    ```bash
    dotnet ef database update

5. Run the application:

    ```bash
    dotnet run

## Usage
- The application supports JWT authorization. Users can register and log in, obtaining tokens to access protected resources.
- API endpoints are available under  ‘/api’, and API documentation can be accessed at  ‘/swagger’.

## Logging
The application uses NLog for logging events. Logs are configured in the NLog.config file.
