# Order Management System

A full-stack order management application built with **.NET 8**, following **Clean Architecture** principles with **CQRS**, **RabbitMQ** messaging, **Blazor Server** frontend, and structured logging via **Serilog**.

---

## Architecture

```
Blazor Server (UI)  ──HTTP──>  ASP.NET Core Web API  ──MediatR──>  Application Layer
                                                                        │
                                                         ┌──────────────┼──────────────┐
                                                      Domain           Infrastructure
                                                   (Entities,        (EF Core, RabbitMQ,
                                                    Interfaces)       Repositories)
```

The solution is organized into five projects with strict inward dependency direction:

| Project | Responsibility |
|---|---|
| **OrderFlow.CQRS.Domain** | Entities, enums, domain events, exceptions, repository interfaces |
| **OrderFlow.CQRS.Application** | Commands, queries, handlers, validators, DTOs, pipeline behaviors |
| **OrderFlow.CQRS.Infrastructure** | EF Core DbContext, repositories, RabbitMQ publisher/consumer |
| **OrderFlow.CQRS.API** | Controllers, middleware, Serilog, Swagger, CORS, health checks |
| **OrderFlow.CQRS.Blazor** | Razor pages, API client service, interactive UI |

---

## Tech Stack

| Category | Technology |
|---|---|
| Platform | .NET 8 (ASP.NET Core) |
| Database | SQL Server 2022 + Entity Framework Core 8 |
| Messaging | RabbitMQ 7 |
| CQRS / Mediator | MediatR 12 |
| Validation | FluentValidation 11 |
| Mapping | AutoMapper 14 |
| Logging | Serilog (Console + File + Seq) |
| Frontend | Blazor Server |
| Infrastructure | Docker Compose |

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

### 1. Start Infrastructure

```bash
docker-compose up -d
```

This starts SQL Server, RabbitMQ, and Seq.

### 2. Build

```bash
dotnet build OrderFlow.CQRS.slnx
```

### 3. Run

```bash
# Terminal 1: API
dotnet run --project src/OrderFlow.CQRS.API --launch-profile https

# Terminal 2: Blazor Frontend
dotnet run --project src/OrderFlow.CQRS.Blazor --launch-profile https
```

### 4. Access

| Service | URL |
|---|---|
| Blazor UI | `https://localhost:7100` |
| API Swagger | `https://localhost:7200/swagger` |
| Health Check | `https://localhost:7200/health` |
| RabbitMQ Management | `http://localhost:15672` (guest/guest) |
| Seq Log Viewer | `http://localhost:8081` |

---

## API Endpoints

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/orders` | List all orders |
| `GET` | `/api/orders/{id}` | Get order by ID |
| `POST` | `/api/orders` | Create new order |
| `PUT` | `/api/orders/{id}/status` | Update order status |
| `DELETE` | `/api/orders/{id}` | Delete order |

See [docs/api-overview.md](docs/api-overview.md) for request/response details.

---

## Project Structure

```
OrderManagement/
├── docker-compose.yml
├── OrderFlow.CQRS.slnx
└── src/
    ├── OrderFlow.CQRS.Domain/
    │   ├── Common/BaseEntity.cs
    │   ├── Entities/           (Order, OrderItem, Product)
    │   ├── Enums/              (OrderStatus, PaymentStatus)
    │   ├── Events/             (OrderCreated, OrderStatusChanged)
    │   ├── Exceptions/         (DomainException, NotFoundException)
    │   └── Interfaces/         (IOrderRepository, IProductRepository)
    │
    ├── OrderFlow.CQRS.Application/
    │   ├── Common/Behaviors/   (ValidationBehavior, LoggingBehavior)
    │   ├── Common/Models/      (Result<T>, PaginatedList<T>)
    │   ├── DTOs/               (OrderDto, ProductDto)
    │   ├── Features/Orders/    (Commands, Queries, EventHandlers)
    │   ├── Interfaces/         (IMessagePublisher)
    │   └── Mappings/           (AutoMapper Profile)
    │
    ├── OrderFlow.CQRS.Infrastructure/
    │   ├── Data/               (DbContext, EF Configurations)
    │   ├── Messaging/          (RabbitMQ Publisher + Consumer)
    │   └── Repositories/       (OrderRepository, ProductRepository)
    |   └── Migrations/         (EF Migrations)
    │
    ├── OrderFlow.CQRS.API/
    │   ├── Controllers/        (OrdersController)
    │   ├── Middleware/         (ExceptionHandler, RequestLogging)
    │   └── Program.cs
    │
    └── OrderFlow.CQRS.Blazor/
        ├── Components/Pages/   (Home, Orders, CreateOrder, OrderDetail)
        └── Services/           (OrderService)
```

---

## Key Features

- **Clean Architecture** with compile-time enforced layer boundaries
- **CQRS** via MediatR with separate command and query handlers
- **Validation Pipeline** using FluentValidation with automatic MediatR integration
- **Global Exception Handling** middleware with structured error responses
- **Asynchronous Messaging** via RabbitMQ with background consumer
- **Domain Events** for loosely coupled event-driven workflows
- **Structured Logging** with Serilog, enriched with request context
- **Centralized Log Aggregation** via Seq
- **Blazor Server Frontend** with interactive order management UI
- **Health Checks** for operational monitoring
- **Swagger/OpenAPI** documentation in development

---

## Order Lifecycle

```
Pending → Confirmed → Processing → Shipped → Delivered
                                       ↘ Cancelled
                                       ↘ Refunded
```

Payment status is tracked independently: `Pending`, `Paid`, `Failed`, `Refunded`.

---

## Documentation

| Document | Description |
|---|---|
| Comming Soon |

---

## Configuration

### API (appsettings.json)

| Setting | Purpose |
|---|---|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string |
| `RabbitMQ:*` | Broker host, port, credentials |
| `AllowedOrigins` | CORS allowed origins for Blazor |
| `Serilog:MinimumLevel` | Logging level configuration |

### Blazor (appsettings.json)

| Setting | Purpose |
|---|---|
| `ApiBaseUrl` | API base URL for HTTP client |

---

## Useful Commands

```bash

dotnet clean

# Build
dotnet build OrderFlow.CQRS.slnx

# Run API
dotnet run --project src/OrderFlow.CQRS.API

# Run Blazor
dotnet run --project src/OrderFlow.CQRS.Blazor

# Create EF migration
dotnet ef migrations add <Name> \
  --project src/OrderFlow.CQRS.Infrastructure \
  --startup-project src/OrderFlow.CQRS.API

# Create initial migrations
dotnet ef migrations add InitialCreate \
  --project src/OrderManagement.Infrastructure \
  --startup-project src/OrderManagement.API

# Apply migrations
dotnet ef database update \
  --project src/OrderFlow.CQRS.Infrastructure \
  --startup-project src/OrderFlow.CQRS.API

# Infrastructure
docker-compose up -d      # Start
docker-compose down        # Stop
docker-compose down -v     # Stop + delete data
```

---

## License

This project is provided as a template for educational and development purposes.
