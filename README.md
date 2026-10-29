# RaceDay

![RaceDay.Api CI](https://github.com/mdkmotsuenyane/RaceDay/actions/workflows/api-ci.yml/badge.svg)
![docs-check](https://github.com/mdkmotsuenyane/RaceDay/actions/workflows/docs-check.yml/badge.svg)

> **🚧 WORK IN PROGRESS – ACADEMIC PROJECT**  
> This project is currently in active development as part of the PROG6212 module. It is not yet deployed to production. Features, endpoints, and schema are subject to change until final submission.

---

## Overview

RaceDay is a full‑stack event management platform designed specifically for the South African road running, walking, and cycling community.

From the iconic **Comrades Marathon** and **Two Oceans Marathon** to the **Cape Town Cycle Tour** and **Soweto Marathon**, the platform allows Event Organisers to create and manage events, categories, and participant results, while Participants can browse upcoming events, enter races, track their personal performance history, and prepare for race day using live weather and route information.

This repository contains a complete **Full-Stack** solution:
1. **RaceDay.Api**: A RESTful **ASP.NET Core Web API** back-end built with Entity Framework Core and SQL Server.
2. **RaceDay.Mvc**: An **ASP.NET Core MVC** front-end that consumes the API, managing state via Session and JWTs.

▶️ **Demo Video Part 1 (Planning):** [Watch on YouTube](https://youtu.be/i0hwWwrlrA4)

▶️ **Demo Video Part 2 (API):** [Watch on YouTube](https://youtu.be/mqsA6EGXBHw)

▶️ **Demo Video Part 3 (Front-End):** Coming soon

---

## 🏗️ Development Status

| Component | Status |
| :--- | :---: |
| Database Design (SQL) | ✅ Complete |
| ERD | ✅ Complete |
| API Endpoint Plan | ✅ Complete |
| Entity Models (EF Core) | ✅ Complete |
| Database Migrations | ✅ Complete |
| Authentication (JWT & Session) | ✅ Complete |
| API Controllers Implementation | ✅ Complete |
| API Unit Tests (xUnit) | ✅ Complete (53 tests) |
| CI/CD (GitHub Actions) | ✅ Complete |
| Front-end Integration (MVC) | ✅ Complete |
| Azure Blob Storage Integration | ✅ Complete |
| UI Styling & Theming | ⏳ In Progress |
| Containerisation (Docker) | ⏳ In Progress |

---

## 🔥 Features Implemented

- **Authentication & Authorisation** – JWT-based API endpoints, with secure Session-based authentication on the MVC front-end.
- **Role-Based Navigation** – Distinct dashboards and interfaces for Organisers and Participants.
- **Event Management** – Organisers can create, edit, and delete events, including uploading event banner images.
- **Category Management** – Create age/distance categories per event.
- **Registration (Entries)** – Participants can enter events (selecting a category) and withdraw. Organisers can view enrolments.
- **Results** – Organisers can upload finish times and positions. Participants can view their personal race history and placement.
- **User Profiles** – Users can update their details and upload profile pictures.
- **Race Day Prep** – Weather forecasts and route information integrated into event details.

---

## 💻 Technology Stack

| Component | Technology |
| :--- | :--- |
| **Framework** | .NET 10 (LTS) |
| **API Architecture** | ASP.NET Core Web API (RESTful) |
| **Front-end Architecture** | ASP.NET Core MVC |
| **ORM** | Entity Framework Core 10 |
| **Database** | Microsoft SQL Server (LocalDB for dev) |
| **Authentication** | JWT (JSON Web Tokens) + Session Cookies |
| **Cloud Storage** | Azure Blob Storage (with local file fallback) |
| **API Documentation** | Swagger / OpenAPI (Swashbuckle) |
| **Password Hashing** | BCrypt.Net |
| **Unit Testing** | xUnit (53 integration tests) |
| **CI/CD** | GitHub Actions |
| **Containerisation** | Docker (In Progress) |

---

## 🔐 Environment Configuration & Azure Blob Storage

This project follows security best practices by **never committing secrets to source control**.

### How the Storage works
The `BlobService` is designed with a fallback strategy:
- **Local Development with Azure:** If a valid `AzureStorage:ConnectionString` is provided in `appsettings.Development.json` (which is gitignored) or User Secrets, the application will upload images to Azure Blob Storage.
- **Fallback (No Keys):** If the connection string is missing (e.g., when a marker clones the repo or runs Docker), the application gracefully falls back to saving images locally in the `wwwroot/uploads` folder. The UI remains fully functional.

To test with your own Azure Storage, create an `appsettings.Development.json` in the `RaceDay.Api` folder and add:
```json
{
  "AzureStorage": {
    "ConnectionString": "YOUR_AZURE_CONNECTION_STRING",
    "ContainerName": "event-images"
  }
}
```

---

## 🚀 Getting Started (Local Development)

### Prerequisites
- [Visual Studio 2022](https://visualstudio.microsoft.com/)
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [SQL Server Express LocalDB](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb)

### 1. Clone the Repository
```bash
git clone https://github.com/mdkmotsuenyane/RaceDay.git
cd RaceDay
```

### 2. Configure the Database & JWT Secret
Open `RaceDay.Api/appsettings.json` and ensure the connection string points to your LocalDB. Then, set your JWT secret via User Secrets:
```bash
cd RaceDay.Api
dotnet user-secrets init
dotnet user-secrets set "Jwt:Key" "<your-32-plus-char-random-secret>"
```

### 3. Apply Database Migrations
```bash
dotnet ef database update -p RaceDay.Api -s RaceDay.Api
```

### 4. Run the Application
Because the MVC app consumes the API, **both projects must run simultaneously**.
1. In Visual Studio, right-click the **Solution** node.
2. Select **Configure Startup Projects...**
3. Choose **Multiple startup projects**.
4. Set both `RaceDay.Api` and `RaceDay.Mvc` to **Start**.
5. Click **OK** and press **F5**.

- **API Swagger:** `https://localhost:7100/swagger`
- **MVC Web App:** `https://localhost:7159`

---

## 🧪 Testing

The API has **53 passing integration tests** covering all controllers, using `WebApplicationFactory<Program>` and isolated in-memory databases.

Run them with:
```bash
dotnet test RaceDay.slnx
```

---

## 📁 Project Structure

```
RaceDay/
├── .github/
│   └── workflows/            # CI workflows (api-ci.yml, docs-check.yml)
├── docs/                     # Planning artifacts (API_Plan.pdf, ERD, endpoints.md)
├── RaceDay.Api/              # ASP.NET Core Web API back-end
│   ├── Controllers/          # API endpoints
│   ├── Data/                 # EF Core DbContext
│   ├── DTOs/                 # Data Transfer Objects
│   ├── Migrations/           # EF Core migrations
│   ├── Models/               # Entity classes
│   ├── Services/             # BlobService (Azure)
│   ├── Program.cs
│   └── appsettings.json
├── RaceDay.Api.Tests/        # xUnit integration test project (53 tests)
├── RaceDay.Mvc/              # ASP.NET Core MVC front-end
│   ├── Controllers/          # UI routing and API consumption
│   ├── Models/               # ViewModels
│   ├── Services/             # ApiClient (HTTP + JWT forwarding)
│   ├── Views/                # Razor UI
│   ├── Program.cs
│   └── appsettings.json
├── RaceDay.slnx              # Solution file
└── README.md
```

---

## ⚙️ Continuous Integration

Two GitHub Actions workflows run on every push to `main`:
- **`docs-check.yml`** – verifies that documentation stays in sync.
- **`api-ci.yml`** – restores, builds, and runs all 53 tests against the API.

[View workflows](https://github.com/mdkmotsuenyane/RaceDay/actions)

---

## ⏭️ Next Milestones

1. ~~Implement JWT Authentication (API).~~ ✅
2. ~~Build all API controllers matching the plan.~~ ✅
3. ~~Write unit/integration tests for API endpoints.~~ ✅
4. ~~Build the MVC front-end that consumes the API.~~ ✅
5. ~~Integrate Azure Blob Storage for image uploads.~~ ✅
6. Polish UI with custom Bootstrap theme.
7. Containerise the application with Docker.

---

## 📃 License

This project is built for educational purposes as part of the PROG6212 module for Rosebank International.

---

## ✍🏾 Contributors

- Mokadi Motsuenyane – ST10480772 Group 3
```
