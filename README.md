# RaceDay 

> **🚧 WORK IN PROGRESS – ACADEMIC PROJECT**  
> This project is currently in active development as part of the PROG6212 module. It is not yet deployed to production. Features, endpoints, and schema are subject to change until final submission.

---

## Overview

RaceDay is a full‑stack event management platform designed specifically for the South African road running, walking, and cycling community. 

From the iconic **Comrades Marathon** and **Two Oceans Marathon** to the **Cape Town Cycle Tour** and **Soweto Marathon**, the platform allows Event Organisers to create and manage events, categories, and participant results, while Participants can browse upcoming events, enter races, track their personal performance history, and prepare for race day using live weather and route information.

This repository contains the **ASP.NET Core Web API** back‑end, built with **Entity Framework Core** and **SQL Server**. The front‑end (MVC) will be built in a later phase.

▶️ **Demo Video:** [Watch on YouTube](https://youtu.be/i0hwWwrlrA4)

---

## 🏗️ Development Status

| Component | Status |
| :--- | :---: |
| Database Design (SQL) | ✅ Complete |
| ERD | ✅ Complete |
| API Endpoint Plan | ✅ Complete |
| Entity Models (EF Core) | ✅ Complete |
| Database Migrations | ✅ Complete |
| Authentication (JWT) | ✅ Complete |
| Controllers Implementation | ✅ Complete |
| Swagger Documentation | ✅ Complete |
| Unit Tests | ⏳ In Progress |
| CI/CD (GitHub Actions) | ⏳ In Progress |
| Front‑end Integration | ⏳ Future Phase |
| Containerisation (Docker) | ⏳ Future Phase |
| Cloud Deployment | ⏳ Future Phase |

---

## 🔥 Features Implemented

- **Authentication & Authorisation** – JWT‑based Register/Login/Refresh for Organisers and Participants.
- **User Profiles** – Unified profile management for both roles via `GET/PUT /api/v1/users/me`.
- **Event Management** – Full CRUD for events, including race day details (weather, route, maps).
- **Category Management** – Create categories per event (e.g. "Men Open", "Ultra Marathon") with age/gender restrictions.
- **Registration (Entries)** – Participants can enter events, with status tracking (Pending/Confirmed/Cancelled).
- **Results** – Organisers can upload results (finish times, positions, chip/gun times) and participants can view their performance.
- **Race Day Preparation** – Weather forecasts and route information integrated directly into event details.

---

## 💻 Technology Stack

| Component | Technology |
| :--- | :--- |
| **Framework** | .NET 10 (LTS) |
| **API Architecture** | ASP.NET Core Web API (RESTful) |
| **ORM** | Entity Framework Core 10 |
| **Database** | Microsoft SQL Server (LocalDB for dev) |
| **Authentication** | JWT (JSON Web Tokens) |
| **API Documentation** | Swagger / OpenAPI (Swashbuckle) |
| **Password Hashing** | BCrypt.Net |
| **Unit Testing** | xUnit (planned) |
| **Containerisation** | Docker (Future) |
| **Cloud** | Azure / AWS compatible (Future) |

---

## Getting Started (Development Environment)

### Prerequisites

- [Visual Studio 2022](https://visualstudio.microsoft.com/) (with ASP.NET and web development workload)
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [SQL Server Express LocalDB](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb) (installed with Visual Studio's ASP.NET workload)
- [SQL Server Object Explorer](https://learn.microsoft.com/en-us/sql/ssms/) (built into Visual Studio)

### 1. Clone the Repository

```bash
git clone https://github.com/mdkmotsuenyane/RaceDay.git
cd RaceDay
