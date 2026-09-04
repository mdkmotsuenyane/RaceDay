# RaceDay 

> **🚧 WORK IN PROGRESS – ACADEMIC PROJECT**  
> This project is currently in active development as part of the PROG6212 module. It is not yet deployed to production. Features, endpoints, and schema are subject to change until final submission.

---

## Overview

RaceDay is a full‑stack event management platform designed specifically for the South African road running, walking, and cycling community. 

From the iconic **Comrades Marathon** and **Two Oceans Marathon** to the **Cape Town Cycle Tour** and **Soweto Marathon**, the platform allows Event Organisers to create and manage events, categories, and participant results, while Participants can browse upcoming events, enter races, track their personal performance history, and prepare for race day using live weather and route information.

This repository contains the **ASP.NET Core Web API** back‑end, built with **Entity Framework Core** and **SQL Server**. The front‑end (MVC / SPA) will be built in a later phase.

---

## Development Status

| Component | Status |
| :--- | :---: |
| Database Design (SQL) | ✅ Complete |
| ERD | ✅ Complete |
| API Endpoint Plan | ✅ Complete |
| Authentication (JWT) | ⏳ Planned |
| Controllers Implementation | ⏳ In Progress |
| Swagger Documentation | ⏳ In Progress |
| Front‑end Integration | ⏳ Future Phase |
| Containerisation (Docker) | ⏳ Future Phase |
| Cloud Deployment | ⏳ Future Phase |

---

## Features (Planned)

- **Authentication & Authorisation** – JWT‑based Register/Login for Organisers and Participants.
- **User Profiles** – Unified profile management for both roles.
- **Event Management** – Full CRUD for events, including race day details (weather, route, maps).
- **Category Management** – Create categories per event (e.g., "Men Open", "Ultra Marathon") with age/gender restrictions.
- **Registration (Entries)** – Participants can enter events, with status tracking (Pending/Confirmed/Cancelled).
- **Results** – Organisers can upload results (finish times, positions, chip/gun times) and participants can view their performance.
- **Race Day Preparation** – Weather forecasts and route information integrated directly into event details.

---

## Technology Stack

| Component | Technology |
| :--- | :--- |
| **Framework** | .NET 8 (LTS) |
| **API Architecture** | ASP.NET Core Web API (RESTful) |
| **ORM** | Entity Framework Core |
| **Database** | Microsoft SQL Server |
| **Authentication** | JWT (JSON Web Tokens) – Planned |
| **API Documentation** | Swagger / OpenAPI |
| **Containerisation** | Docker (Future) |
| **Cloud** | Azure / AWS compatible (Future) |

---

## Getting Started (Development Environment)

### Prerequisites

- [Visual Studio 2022](https://visualstudio.microsoft.com/) (with ASP.NET and web development workload)
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (Developer or Express edition)
- [SQL Server Management Studio (SSMS)](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms)

### 1. Clone the Repository

```bash
git clone https://github.com/mdkmotsuenyane/RaceDay.git
cd RaceDay
```

### 2. Set Up the Database

**Recommended:** Run the `RaceDayDB.sql` script located in the project root.

1. Open **SSMS** and connect to your local SQL Server instance.
2. Open `RaceDayDB.sql` and execute the entire script. This will:
   - Create the `RaceDayDB` database.
   - Create all tables (`User`, `Organiser`, `Participant`, `Event`, `Category`, `Entry`, `Result`).
   - Seed the database with test data (4 Organisers, 8 Participants, 4 Events, 8 Categories, 10 Entries, 5 Results).

> **Note:** The SQL script is the single source of truth for the schema and seed data during development.

### 3. Configure the Connection String

Open `appsettings.json` and update the `DefaultConnection` to point to your SQL Server instance:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=RaceDayDB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "AllowedHosts": "*"
}
```

### 4. Restore Packages & Build

```bash
dotnet restore
dotnet build
```

### 5. Run the API Locally

Press **F5** in Visual Studio or run:

```bash
dotnet run
```

The API will launch, and **Swagger** will open automatically at `https://localhost:{port}/swagger` (if configured) for testing endpoints.

> **Important:** Authentication is not yet implemented – endpoints are currently unprotected for local development. JWT will be added in the next milestone.

---

## Test Data Credentials (For Future Auth Testing)

All seeded users share the same password for testing purposes:

> **Password:** `Password123`

---

## API Endpoint Plan

The complete API endpoint plan (39 endpoints) is documented in the [`/docs/API_Plan.pdf`](./docs/API_Plan.pdf) file. It includes:

- Authentication (Register, Login, Refresh)
- User Profiles
- Organisers CRUD
- Participants CRUD
- Events CRUD (including Weather & Route sub‑endpoints)
- Categories CRUD
- Entries (Registrations) CRUD
- Results CRUD

---

## Database Schema (ERD)

The Entity Relationship Diagram is available at [`/docs/RaceDayERD.png`](./docs/RaceDayERD.png).

The core entities are:

- **User** – Authentication and base profile (linked 1‑to‑1 to Organiser or Participant).
- **Organiser** – Creates and manages Events.
- **Participant** – Enters Events via Entries.
- **Event** – Contains weather and route details for race day prep.
- **Category** – Distances, age groups, and gender restrictions per Event.
- **Entry** – Junction table linking Participants to Events/Categories.
- **Result** – Tied to an Entry for finish times and positions.

---

## Project Structure

```
RaceDay.Api/
├── Controllers/          # API Controllers (to be added)
├── Data/                 # AppDbContext
├── Models/               # Entity Framework models
│   └── DTOs/             # Request/Response DTOs
├── Migrations/           # EF Core migrations (auto-generated)
├── Program.cs            # Application entry point
├── appsettings.json      # Configuration
└── RaceDayDB.sql         # Database schema + seed data
```

---

## Known Limitations (Work in Progress)

- ❌ Authentication (JWT) not yet implemented.
- ❌ Controllers not yet coded – only endpoint plan and database are complete.
- ❌ No front‑end application yet.
- ❌ No containerisation or deployment configuration.
- ✅ Database schema and seed data are complete and stable.

---

## Next Milestones

1. Implement JWT Authentication.
2. Build all controllers matching the API plan.
3. Test all endpoints via Swagger.
4. Build the MVC front‑end that consumes the API.
5. Containerise with Docker.

---

## License

This project is built for educational purposes as part of the PROG6212 module for Rosebank International.

---

## Contributors

- Mokadi Motsuenyane – ST10480772 Group 3

---

