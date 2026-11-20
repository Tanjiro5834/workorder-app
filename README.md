# 🛠️ Work Order and Incident Report System

A full-stack work order and incident tracking app, built to learn **ASP.NET Core** end to end.

![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-Web_API-512BD4?logo=dotnet&logoColor=white)
![EF Core](https://img.shields.io/badge/EF_Core-ORM-6DB33F)
![SQL Server](https://img.shields.io/badge/SQL_Server-CC2927?logo=microsoftsqlserver&logoColor=white)
![Vue](https://img.shields.io/badge/Vue-4FC08D?logo=vuedotjs&logoColor=white)
![TypeScript](https://img.shields.io/badge/TypeScript-3178C6?logo=typescript&logoColor=white)
![Vite](https://img.shields.io/badge/Vite-646CFF?logo=vite&logoColor=white)

---

## 📌 Overview

Work Order App lets teams create, track, and move work orders through a simple status workflow. Every record belongs to a branch. The API handles validation and pagination on the server, and the React client stays thin.

## ✨ Features

- **Work order CRUD**: create, view, update, and delete work orders
- **Status workflow**: `Open` → `InProgress` → `Done`
- **Filtering**: filter work orders by status
- **Server-side pagination**: the server sends only the page you need, so large lists stay fast
- **Validation**: errors are shown next to the field that caused them
- **Branch-based records**: each work order is tied to a branch

## 🧱 Tech Stack

| Layer    | Technology                                  |
| -------- | ------------------------------------------- |
| Backend  | ASP.NET Core Web API (.NET 10)              |
| ORM      | Entity Framework Core                       |
| Database | SQL Server                                  |
| Frontend | React + TypeScript (Vite)                   |

## 📁 Project Structure

```
.
├── WorkOrderApi/   # ASP.NET Core Web API + EF Core
└── web/            # React + TypeScript client (Vite)
```

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (LTS)
- SQL Server (LocalDB, Express, or Docker)
- EF Core CLI tools:
  ```bash
  dotnet tool install --global dotnet-ef
  ```

### 1. Run the API

```bash
cd WorkOrderApi
# Point the connection string in appsettings.json to your SQL Server instance
dotnet ef database update
dotnet run
```

### 2. Run the Web Client

```bash
cd web
npm install
npm run dev
```

## 🗺️ Roadmap

- [x] Work order CRUD + status workflow
- [x] Status filter + server-side pagination
- [x] Field-level validation
- [ ] JWT authentication + roles (`Admin` / `Staff`)
- [ ] Branch-scoped access
- [ ] Incident reports

## 🎯 Learning Goals

I built this project to get hands-on with:

- Designing a RESTful API in ASP.NET Core
- EF Core migrations and data modeling
- Server-side pagination and filtering
- Consistent validation error responses for the frontend
- Connecting a typed React client to a .NET backend
