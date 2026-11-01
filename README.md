# iNDLELA (RaceDay Architecture)

![RaceDay.Api CI](https://github.com/mdkmotsuenyane/RaceDay/actions/workflows/api-ci.yml/badge.svg)
![docs-check](https://github.com/mdkmotsuenyane/RaceDay/actions/workflows/docs-check.yml/badge.svg)


---

## Overview

**iNDLELA** (The Path) is a full-stack event management platform designed specifically for the South African road running, walking, and cycling community. 

> *Note: The technical codebase, API, and database are named `RaceDay` as per the project specification. However, the user-facing platform has been branded as **iNDLELA** to create a strong, energetic sporting identity, fulfilling the UI/UX branding requirements of the module.*

This repository contains a complete **Full-Stack** solution:
1. **RaceDay.Api**: A RESTful **ASP.NET Core Web API** back-end built with Entity Framework Core and SQL Server.
2. **RaceDay.Mvc**: An **ASP.NET Core MVC** front-end that consumes the API, managing state via Session and JWTs.

▶️ **Demo Video Part 1 (Planning):** [Watch on YouTube](https://youtu.be/i0hwWwrlrA4)

▶️ **Demo Video Part 2 (API):** [Watch on YouTube](https://youtu.be/mqsA6EGXBHw)

▶️ **Demo Video Part 3 (Front-End & Docker):** Coming soon

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
| UI Styling & Theming (iNDLELA) | ✅ Complete |
| Containerisation (Docker) | ✅ Complete |

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
| **Containerisation** | Docker & Docker Compose |

---

## 🐳 Docker Setup

The application is fully containerised using Docker. Both the API and the MVC front-end, along with a SQL Server database, can be started with a single command.

### Prerequisites
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) must be installed and running.

### Run the application
Navigate to the root of the repository (where `docker-compose.yml` is located) and run:

```bash
docker compose up --build
```

### Access the containers
- **iNDLELA Web App:** http://localhost:7159
- **API Swagger:** http://localhost:7100/swagger

*(Note: The API is configured to automatically apply Entity Framework migrations on startup, so the database schema will be created automatically inside the container. Please allow a few seconds for SQL Server to boot up before the API connects).*

---

## 🔐 Environment Configuration & Azure Blob Storage

This project follows security best practices by **never committing secrets to source control**.

### How the Storage works
The `BlobService` is designed with a robust fallback strategy:
- **Local Development with Azure:** If a valid `AzureStorage:ConnectionString` is provided in `appsettings.Development.json` (which is gitignored) or User Secrets, the application will upload images directly to Azure Blob Storage.
- **Fallback (No Keys):** If the connection string is missing (e.g., when a marker clones the repo or runs Docker), the application gracefully catches the error and falls back to saving images locally in the `wwwroot/uploads` folder. The UI remains fully functional.

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

- **API Swagger:** `http://localhost:5100/swagger`
- **iNDLELA Web App:** `http://localhost:5103`

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
│   ├── Services/             # BlobService (Azure & Local Fallback)
│   ├── Program.cs
│   └── appsettings.json
├── RaceDay.Api.Tests/        # xUnit integration test project (53 tests)
├── RaceDay.Mvc/              # ASP.NET Core MVC front-end (iNDLELA Brand)
│   ├── Controllers/          # UI routing and API consumption
│   ├── Models/               # ViewModels
│   ├── Services/             # ApiClient (HTTP + JWT forwarding)
│   ├── Views/                # Razor UI (Custom iNDLELA Theme)
│   ├── Program.cs
│   └── appsettings.json
├── RaceDay.slnx              # Solution file
├── docker-compose.yml        # Multi-container orchestration
└── README.md
```

---

## ⚙️ Continuous Integration

Two GitHub Actions workflows run on every push to `main`:
- **`docs-check.yml`** – verifies that documentation stays in sync.
- **`api-ci.yml`** – restores, builds, and runs all 53 tests against the API.

[View workflows](https://github.com/mdkmotsuenyane/RaceDay/actions)

---

## 📃 License

This project is built for educational purposes as part of the PROG6212 module for Rosebank International.

---

## ✍🏾 Contributors

- Mokadi Motsuenyane – ST10480772 Group 3
```
