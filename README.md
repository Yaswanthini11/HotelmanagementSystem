# Hotel Management System

## HCL Training – Day 36

A full-stack Hotel Management System developed using ASP.NET Core Web API, Entity Framework Core, SQL Server, and Angular.

## 1. Project Overview

The Hotel Management System is designed to manage hotel operations, including hotel information, amenities, room types, room inventory, customer authentication, and bookings.

The application provides a backend REST API and an Angular frontend for interacting with the system.

## 2. Technologies Used

### Backend
- C#
- ASP.NET Core Web API (.NET 10)
- Entity Framework Core 10
- SQL Server
- ASP.NET Core Identity
- JWT Authentication and Authorization
- Repository Pattern
- Dependency Injection

### Frontend
- Angular
- TypeScript
- HTML
- CSS
- Angular Services
- HTTP Client

### Development Tools
- Visual Studio / Visual Studio Code
- SQL Server Express
- SQL Server Management Studio
- Git and GitHub
- Postman
- Node.js and npm

## 3. Project Structure

```text
HotelmanagementSystem/
│
├── Backend/
│   └── HotelBooking/
│       ├── Controllers/
│       ├── Data/
│       ├── DTOs/
│       ├── Migrations/
│       ├── Models/
│       ├── Repositories/
│       ├── Services/
│       ├── Program.cs
│       ├── appsettings.json
│       └── HotelBooking.csproj
│
├── HotelManagement.Frontend/
│   ├── src/
│   ├── public/
│   ├── angular.json
│   ├── package.json
│   └── tsconfig.json
│
└── README.md
```

## 4. Main Features

- User registration and login
- JWT-based authentication
- Role-based authorization
- Hotel management
- Hotel image management
- Amenity management
- Room type management
- Room inventory management
- Booking management
- Database integration using Entity Framework Core

## 5. Backend Setup

### Prerequisites

Install:
- .NET 10 SDK
- SQL Server Express
- SQL Server Management Studio (optional)

### Step 1: Configure the database

Configure the `DefaultConnection` connection string using User Secrets or the appropriate local configuration.

Example SQL Server Express connection string:

```text
Server=.\SQLEXPRESS;Database=HotelBookingDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true
```

### Step 2: Configure application secrets

Initialize User Secrets if they have not already been initialized:

```cmd
dotnet user-secrets init --project Backend\HotelBooking\HotelBooking.csproj
```

Configure the following keys with your own values:

```text
DefaultConnection
JwtSettings:SecretKey
JwtSettings:Issuer
JwtSettings:Audience
AdminBootstrap:Email
AdminBootstrap:Password
AdminBootstrap:FullName
```

Keep passwords and JWT signing keys private. Do not commit them to GitHub.

### Step 3: Restore packages

```cmd
dotnet restore Backend\HotelBooking\HotelBooking.csproj
```

### Step 4: Apply database migrations

```cmd
dotnet ef database update --project Backend\HotelBooking\HotelBooking.csproj --startup-project Backend\HotelBooking\HotelBooking.csproj
```

### Step 5: Build the backend

```cmd
dotnet build Backend\HotelBooking\HotelBooking.csproj
```

### Step 6: Run the backend

```cmd
dotnet run --project Backend\HotelBooking\HotelBooking.csproj --urls http://localhost:5041
```

Backend address:

```text
http://localhost:5041
```

## 6. Frontend Setup

### Prerequisites

Install:
- Node.js
- npm

### Step 1: Navigate to the frontend

```cmd
cd HotelManagement.Frontend
```

### Step 2: Install dependencies

```cmd
npm install
```

### Step 3: Build the Angular application

```cmd
npm run build
```

### Step 4: Run the frontend

```cmd
npx ng serve --port 4200
```

Frontend address:

```text
http://localhost:4200
```

Keep the backend and frontend running in separate terminals.

## 7. Testing

### Backend testing

Use Postman to test:
- User registration
- User login
- Hotel operations
- Amenity operations
- Room type operations
- Room inventory operations
- Booking operations
- Authentication and authorization

Use a valid JWT token for protected API endpoints.

### Frontend testing

Verify:
- Application launches successfully.
- Registration and login work.
- Authenticated users can access permitted pages.
- Hotel and room information loads correctly.
- Booking operations work as expected.

## 8. Build Verification

The following builds were successfully verified during integration:

| Component | Result |
|---|---|
| Backend .NET 10 | Successful |
| Backend warnings | 0 |
| Backend errors | 0 |
| Angular frontend | Successful |
| Angular initial bundle | 414.43 kB |
| Git merge conflicts | Resolved |

## 9. GitHub Repository

Team repository:

https://github.com/Yaswanthini11/HotelManagementSystem

Integration branch:

`backup/hotelmanagement-local-work`

Integration commit:

`aa869af`

The integration branch was merged into the team's `master` branch through a Pull Request.

## 10. Learning Outcomes

Through this project, I gained practical experience in:

- Developing RESTful APIs using ASP.NET Core.
- Implementing authentication and role-based authorization.
- Working with Entity Framework Core and SQL Server.
- Applying the Repository Pattern.
- Building frontend applications using Angular.
- Integrating Angular with ASP.NET Core APIs.
- Managing database migrations.
- Resolving Git merge conflicts.
- Working with feature branches and Pull Requests.
- Building and verifying a full-stack application.

---

**Project:** Hotel Management System  
**Training:** HCL  
**Day:** 36  
**Status:** Backend and frontend integration completed; team repository PR merged.
