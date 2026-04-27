# 💠 NetHexaGen

**The Ultimate Enterprise-Grade Scaffolding Engine for .NET 10.**

NetHexaGen is a code generation engine designed for architects and developers who want to create robust, scalable, and maintainable solutions based on **Clean Architecture** and **Domain-Driven Design (DDD)** principles.

[![.NET 10](https://img.shields.io/badge/.NET-10.0-blueviolet.svg)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![Clean Architecture](https://img.shields.io/badge/Architecture-Clean-green.svg)](#)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

---

## 🚀 Why NetHexaGen?

Setting up an enterprise solution from scratch takes days. NetHexaGen does it in **seconds**, integrating the industry's most demanding standards by default:

- **Clean Architecture**: 5 perfectly decoupled layers (Domain, Application, Infrastructure, Api, Tests).
- **Interactive CLI**: Visual menu to choose between Controllers vs Minimal APIs and DB providers (SQL Server, PostgreSQL, SQLite).
- **Security First**: Out-of-the-box configuration for JWT Bearer and CORS policies.
- **Architecture Guard**: Automated architectural tests with NetArchTest.
- **Modern UI**: API documentation with Scalar (the evolution of Swagger).

---

## 🛠️ Installation

Currently, you can run it directly from the source code or compile it as a global tool:

```bash
# Clone and build
dotnet build NetHexaGen.csproj
```

---

## 📖 Quick Commands

### 1. Create a New Solution
Starts an interactive wizard to configure your project:

```bash
nethexagen new MyVortexProject
```
*It will ask you for the API style and the database provider of your choice.*

### 2. Add a Resource (CRUD)
Generates the entire technology stack for a new entity:

```bash
nethexagen add User
```
*Generates: Entity, Commands, Handlers, and Controller/Endpoint automatically.*

### 🗄️ Database Migrations
Generated projects are ready for Entity Framework Core. To apply migrations:
```bash
dotnet ef database update --project YourProject.Infrastructure --startup-project YourProject.Api
```

---

## 🏛️ SOLID Principles Implementation

NetHexaGen is built from the ground up to enforce **SOLID** principles:

- **S - Single Responsibility**: Each layer and class has a single, well-defined purpose.
- **O - Open/Closed**: New features can be added via MediatR handlers without modifying existing core logic.
- **L - Liskov Substitution**: Use of generic abstractions ensures consistent behavior across implementations.
- **I - Interface Segregation**: Small, focused interfaces (e.g., `ICommand`, `IRepository`) prevent bloated dependencies.
- **D - Dependency Inversion**: High-level modules (Domain/Application) are completely independent of low-level implementation details (Infrastructure/Api).

---

## 🏗️ The Anatomy of a NetHexaGen Project

1. **Domain**: The core of the business. DDD primitives, Result Pattern, and pure Entities.
2. **Application**: Use cases with MediatR, Validations with FluentValidation, and Mappings with Mapster.
3. **Infrastructure**: Persistence with EF Core, Repository Pattern, and Unit of Work.
4. **Api**: Scalar documentation, Versioning, Health Checks, and JWT Security.
5. **ArchitectureTests**: The guardian that ensures layer dependency rules are never violated.

---

## 💎 Included Features

| Feature | Technology |
| :--- | :--- |
| **Architecture** | Clean Architecture, CQRS, DDD |
| **Logging** | Serilog (File & Console) |
| **Validation** | FluentValidation (Automatic Pipeline) |
| **Mapping** | Mapster |
| **Documentation** | Scalar (OpenAPI 3.0) |
| **Health Checks** | Health Checks (DB & App) |
| **Containers** | Docker (Multi-stage) |
| **Consistency** | .editorconfig |

---

## 🤝 Contributing

Have an idea for a new template or an architectural improvement? Pull Requests are welcome!

Developed by **rbaezc**. Licensed to **Vortex Solutions**.
