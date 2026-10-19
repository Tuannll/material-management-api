<div align="center">

# 🏗️ Construction Material Management API

**Production-ready ASP.NET Core 10 Web API managing construction materials with deep comparative analysis of LINQ Method vs. Query Syntax.**

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![.NET Build](https://github.com/Tuannll/material-management-api/actions/workflows/dotnet.yml/badge.svg)](https://github.com/Tuannll/material-management-api/actions/workflows/dotnet.yml)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-Web%20API-blue?logo=csharp)](https://learn.microsoft.com/aspnet/core)
[![Entity Framework Core](https://img.shields.io/badge/EF%20Core-10.0-6C257E?logo=nuget)](https://learn.microsoft.com/ef/core)
[![Swagger](https://img.shields.io/badge/OpenAPI-Swagger-85EA2D?logo=swagger&logoColor=black)](https://swagger.io/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

**English** | [Tiếng Việt](README.vi.md)

**[✨ Features](#features) • [📁 Structure](#structure) • [🚀 Quick Start](#quick-start) • [📡 Endpoints](#endpoints) • [💡 LINQ Analysis](#linq-analysis) • [📄 License](#license)**

</div>

---

A RESTful Web API managing construction material inventories and suppliers, engineered according to Clean Architecture principles with ASP.NET Core 10 and Entity Framework Core. The project offers a side-by-side benchmark of **LINQ Method Syntax** and **LINQ Query Syntax**, supporting dynamic runtime resolution, robust pagination, and centralized error handling.

---

<a id="features"></a>
## 🌟 Key Features

- **Clean Architecture & Separation of Concerns**: Explicit modularization into Controllers, Services, DTOs, Domain Entities, and DbContext.
- **Dual LINQ Implementations**:
  - `MethodSyntaxMaterialService`: Implemented via Fluent extension methods (`Join`, `GroupJoin`, `SelectMany`, `Where`).
  - `QuerySyntaxMaterialService`: Implemented via declarative query comprehension (`from`, `join ... into`, `where`, `select`).
  - **Dynamic Resolver**: Toggle LINQ execution styles at runtime via query string or HTTP header.
  - **Syntax Comparison Endpoint (`/syntax-comparison`)**: Concurrently executes both pipelines to verify output equivalence.
- **Advanced Relational Joins**:
  - `INNER JOIN`: Filters exclusively for materials associated with an active supplier.
  - `LEFT OUTER JOIN`: Preserves all materials; unassigned suppliers default gracefully to `"Chưa có nhà cung cấp"`.
- **Safe Pagination & Filtering**:
  - `PagingDto<T>`: Returns complete metadata: `PageIndex`, `PageSize`, `TotalPages`, `TotalItems`, `HasPreviousPage`, `HasNextPage`.
  - `FilterMaterialDto`: Auto-trims search queries and enforces non-negative price boundaries (`MinPrice <= MaxPrice`) via `IValidatableObject`.
- **Global Exception Handling**:
  - Intercepts uncaught exceptions via middleware and standardizes envelopes as `ApiResponse<T>`.
  - Auto-captures model state validation errors and emits structured HTTP 400 responses.
- **Resilient Database Layer**:
  - Automatically targets SQL Server when configured.
  - Fallbacks seamlessly to an In-Memory Database (`TH4_Db`) for instant zero-dependency testing.
  - Single-execution seed initialization (`SeedData`) at startup via `DbInitializer`.

---

<a id="structure"></a>
## 📁 Project Structure

```text
material-management-api/
├── Controllers/
│   └── MaterialsController.cs         # REST API endpoints, routing, OpenAPI annotations
├── Data/
│   ├── ApplicationDbContext.cs        # EF Core DbContext, Fluent API mapping
│   ├── DbInitializer.cs               # Database startup seeding logic
│   └── SeedData.cs                    # Mock seed records (4 suppliers, 10 materials)
├── Exceptions/
│   ├── NotFoundException.cs           # HTTP 404 domain exception
│   └── UserFriendlyException.cs       # HTTP 400 business exception with custom error codes
├── Middleware/
│   └── GlobalExceptionMiddleware.cs   # Global exception middleware with camelCase JSON serialization
├── Models/
│   ├── Common/
│   │   ├── ApiResponse.cs             # API response envelope
│   │   └── ErrorCodes.cs              # Business error code constants
│   ├── Dtos/
│   │   ├── CreateMaterialDto.cs       # Material creation DTO
│   │   ├── UpdateMaterialDto.cs       # Material update DTO
│   │   ├── MaterialDto.cs             # Material list item DTO
│   │   ├── MaterialDetailDto.cs       # Detailed material & supplier DTO
│   │   ├── FilterDto.cs               # Base pagination DTO
│   │   ├── FilterMaterialDto.cs       # Price & keyword search DTO
│   │   └── PagingDto.cs               # Generic pagination response metadata
│   └── Entities/
│       ├── Material.cs                # Material entity
│       └── Supplier.cs                # Supplier entity
├── Services/
│   ├── Interfaces/
│   │   └── IMaterialService.cs        # Material service contract
│   └── Implementations/
│       ├── MethodSyntaxMaterialService.cs # LINQ Method Syntax implementation
│       └── QuerySyntaxMaterialService.cs  # LINQ Query Syntax implementation
├── GiaiThich-Inner.txt                # Comprehensive theoretical comparison on Inner vs Left Joins
└── Program.cs                         # Bootstrapper, DI container, and middleware pipeline
```

---

<a id="quick-start"></a>
## 🚀 Quick Start

### Prerequisites
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or higher.
- *(Optional)* SQL Server instance if persistent storage is desired.

> [!TIP]
> The application automatically falls back to an In-Memory Database (`TH4_Db`) if no valid SQL Server connection is supplied. You can clone and run immediately without database setup.

### 1. Clone & Build

```bash
git clone https://github.com/Tuannll/material-management-api.git
cd material-management-api
dotnet restore
dotnet build
```

### 2. Run Application

```bash
dotnet run
```

### 3. Open Swagger UI
Open your browser at: `http://localhost:5018/swagger` or `http://localhost:5000/swagger`.

---

<a id="endpoints"></a>
## 📡 API Endpoints

| Method | Endpoint | Description | HTTP Status |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/materials` | Retrieve paginated & filtered materials | `200`, `400` |
| `GET` | `/api/materials/{id}` | Get material details by ID | `200`, `404` |
| `GET` | `/api/materials/inner-join` | Query materials via INNER JOIN | `200` |
| `GET` | `/api/materials/left-join` | Query materials via LEFT JOIN | `200` |
| `GET` | `/api/materials/syntax-comparison` | Benchmark both LINQ implementations side-by-side | `200` |
| `POST` | `/api/materials` | Create material | `201`, `400` |
| `PUT` | `/api/materials/{id}` | Update material | `200`, `400`, `404` |
| `DELETE` | `/api/materials/{id}` | Delete material | `204`, `404` |

---

<a id="linq-analysis"></a>
## 💡 In-Depth LINQ Testing Guide

### 1. Dynamic Syntax Switching
Switch between service implementations per request:
- **Option 1: Query String**: Append `?syntax=query` or `?syntax=method`
  ```http
  GET /api/materials?syntax=query
  ```
- **Option 2: Request Header**: Pass `X-Syntax-Style: query` or `X-Syntax-Style: method`
  ```bash
  curl -X GET "http://localhost:5018/api/materials" -H "X-Syntax-Style: query"
  ```
The `sourceImplementation` field in the response envelope indicates `"Method Syntax Style"` or `"Query Syntax Style"`.

### 2. INNER JOIN vs. LEFT OUTER JOIN Verification
- **Sample Data**: 10 materials seeded:
  - IDs 1 to 8: Assigned to valid suppliers (1, 2, 3, or 4).
  - IDs 9 (`Ống nhựa PVC`) and 10 (`Sơn chống thấm`): `SupplierId = null`.
- **Inner Join Test**:
  ```http
  GET /api/materials/inner-join
  ```
  -> Returns **8 records** (IDs 9 and 10 are excluded).
- **Left Join Test**:
  ```http
  GET /api/materials/left-join
  ```
  -> Returns all **10 records** (IDs 9 and 10 output `supplierName: "Chưa có nhà cung cấp"`).

---

<a id="license"></a>
## 📄 License

Distributed under the [MIT License](LICENSE).
