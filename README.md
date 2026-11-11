# Work Order App

Full-stack work order and incident tracking app built to learn ASP.NET Core.

## Stack
- **Backend:** ASP.NET Core Web API (.NET 10), Entity Framework Core, SQL Server
- **Frontend:** React + TypeScript (Vite)

## Features
- Work order CRUD with status workflow (Open → InProgress → Done)
- Filter by status, server-side pagination
- Validation with field-level error messages
- Branch-based records

## Run locally
```bash
# API
cd WorkOrderApi
dotnet ef database update
dotnet run

# Web
cd web
npm install
npm run dev
```

## Roadmap
- [ ] JWT authentication + roles (Admin/Staff)
- [ ] Branch-scoped access
- [ ] Incident reports
