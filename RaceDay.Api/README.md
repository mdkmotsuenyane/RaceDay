\# RaceDay.Api



ASP.NET Core Web API for the RaceDay platform.



\## Layout



\- `Controllers/` — API endpoints (Auth, Users, Organisers, Participants, Events, Categories, Entries, Results)

\- `Data/` — `RaceDayDbContext` (EF Core)

\- `DTOs/` — request/response shapes

\- `Migrations/` — EF Core migrations

\- `Models/` — entity classes



\## Run



```bash

cd RaceDay.Api

dotnet user-secrets init

dotnet user-secrets set "Jwt:Key" "<32+ char random string>"

dotnet ef database update

dotnet run

