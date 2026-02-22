# Cà Phê Văn Phòng

A Vietnamese office coffee service platform built with **Blazor Server-Side Rendering (.NET 8+)**, following **Clean Architecture** principles, using **Entity Framework Core** and **Microsoft SQL Server**.

## 📖 Project Overview

**Cà Phê Văn Phòng** (Office Coffee) is a modern web application for a coffee machine rental and premium coffee bean supply service targeting office customers. The business provides high-quality coffee machines for rent along with premium coffee beans to offices, enabling them to enjoy barista-quality coffee in their workplace.

The application provides two distinct areas:

- **Public Area** — Accessible to all visitors for browsing coffee machine rental options, viewing available coffee bean products, and placing orders. Authenticated users with the `User` role can view and manage their own orders.
- **Admin Area** — Restricted to `Admin` and `SuperAdmin` roles for full management of products, machines, orders, and customers.

### Key Features

- ☕ **Coffee Machine Rental** — Browse and rent premium coffee machines for office use
- 🫘 **Premium Coffee Beans** — Order high-quality coffee beans delivered to your office
- 📋 **Order Management** — Place, track, and manage rental and product orders
- 👤 **User Authentication** — ASP.NET Core Identity with role-based access control
- 📱 **Responsive Design** — Mobile-first approach for all pages

---

## 🏗️ Architecture

This project follows **Clean Architecture** (also known as Onion Architecture), ensuring separation of concerns and maintainability.

### Solution Structure

```
caphevanphong/
├── src/
│   ├── Core/
│   │   ├── CapheVanPhong.Domain/          # Entities, Value Objects, Domain Events, Interfaces
│   │   └── CapheVanPhong.Application/     # Use Cases, DTOs, Interfaces, Services, Validators
│   ├── Infrastructure/
│   │   └── CapheVanPhong.Infrastructure/  # EF Core DbContext, Repositories, Migrations, Identity
│   └── Presentation/
│       └── CapheVanPhong.Web/
│           ├── Components/
│           │   ├── Pages/
│           │   │   ├── Public/            # Public-facing pages (menu, home, order tracking)
│           │   │   └── Admin/             # Admin-only pages (dashboard, management)
│           │   ├── Shared/                # Shared/reusable components
│           │   └── Account/               # Login, Register, Logout pages
│           └── Layout/
│               ├── PublicLayout.razor     # Layout for public area (Bootstrap 4)
│               └── AdminLayout.razor      # Layout for admin area (Bootstrap 3)
└── tests/
    ├── CapheVanPhong.Domain.Tests/
    ├── CapheVanPhong.Application.Tests/
    └── CapheVanPhong.Infrastructure.Tests/
```

### Layer Responsibilities

| Layer | Responsibilities |
|-------|-----------------|
| **Domain** | Entities, Value Objects, Enums, Domain Events, Repository Interfaces |
| **Application** | Use Cases (CQRS with MediatR), DTOs, Validators (FluentValidation), Services |
| **Infrastructure** | EF Core DbContext, Repository implementations, Migrations, Identity |
| **Presentation** | Blazor SSR components, DI registration, Authentication UI |

### Technology Stack

| Component | Technology |
|-----------|------------|
| Framework | Blazor Server-Side Rendering (.NET 10) |
| Architecture | Clean Architecture |
| ORM | Entity Framework Core |
| Database | Microsoft SQL Server |
| Authentication | ASP.NET Core Identity with RBAC |
| UI Framework | Bootstrap 4 (Public) / Bootstrap 3 (Admin) |
| CQRS | MediatR |
| Validation | FluentValidation |
| Logging | Serilog (Console + File + Email on Error) |

---

## 👥 User Roles & Authorization

| Role | Description |
|------|-------------|
| `SuperAdmin` | Highest privilege. Can manage everything including SuperAdmin accounts. |
| `Admin` | Full Admin area access. Can manage products, machines, orders, and non-SuperAdmin users. |
| `User` | Authenticated customer. Can view and edit their own orders. |
| *(anonymous)* | Can browse available products and machines. |

---

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- [Microsoft SQL Server](https://www.microsoft.com/sql-server/) (LocalDB, Express, or full instance)
- [Git](https://git-scm.com/)

### Database Configuration

1. Update the connection string in `src/Presentation/CapheVanPhong.Web/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CapheVanPhong;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### Email Configuration (for Error Notifications)

The application uses Serilog to send error logs via email. To enable this feature, you need to configure SMTP credentials using **User Secrets** (development) or environment variables (production).

#### Development Setup (User Secrets)

Run the following command to add your SMTP password to User Secrets:

```bash
cd src/Presentation/CapheVanPhong.Web
dotnet user-secrets init
dotnet user-secrets set "Email:SmtpPassword" "your-16-char-app-password"
```

Or manually edit the secrets file:

```bash
# Open secrets in your editor
dotnet user-secrets edit
```

Add the following content:

```json
{
  "Email": {
    "SmtpPassword": "your-16-char-app-password"
  }
}
```

> **Note:** The SMTP password should be a 16-character app password generated from your email provider (e.g., Google App Password for Gmail).

#### Production Setup

Set the environment variable `Cpvp_Email_SmtpPassword` on your production server:

```bash
# Linux/macOS
export Cpvp_Email_SmtpPassword="your-16-char-app-password"

# Windows (Command Prompt)
set Cpvp_Email_SmtpPassword=your-16-char-app-password

# Windows (PowerShell)
$env:Cpvp_Email_SmtpPassword = "your-16-char-app-password"
```

### Useful Commands

#### Run the Application

```bash
# Navigate to the Web project and run
dotnet run --project src/Presentation/CapheVanPhong.Web

# Or run from root with hot reload
dotnet watch --project src/Presentation/CapheVanPhong.Web
```

#### Entity Framework Core Migrations

```bash
# Add a new migration
dotnet ef migrations add <MigrationName> --project src/Infrastructure/CapheVanPhong.Infrastructure --startup-project src/Presentation/CapheVanPhong.Web

# Apply migrations to database
dotnet ef database update --project src/Infrastructure/CapheVanPhong.Infrastructure --startup-project src/Presentation/CapheVanPhong.Web

# Rollback to a specific migration
dotnet ef database update <MigrationName> --project src/Infrastructure/CapheVanPhong.Infrastructure --startup-project src/Presentation/CapheVanPhong.Web

# Remove the last migration (before applying)
dotnet ef migrations remove --project src/Infrastructure/CapheVanPhong.Infrastructure --startup-project src/Presentation/CapheVanPhong.Web

# Generate SQL script for migrations
dotnet ef migrations script --project src/Infrastructure/CapheVanPhong.Infrastructure --startup-project src/Presentation/CapheVanPhong.Web
```

#### Build and Test

```bash
# Build the entire solution
dotnet build

# Run all tests
dotnet test

# Run specific test project
dotnet test tests/CapheVanPhong.Application.Tests
```

#### Clean and Restore

```bash
# Clean build outputs
dotnet clean

# Restore NuGet packages
dotnet restore

# Publish for deployment
dotnet publish src/Presentation/CapheVanPhong.Web -c Release -o ./publish
```

---

## 🌐 Language & UI

- **Public Area**: All text in **Vietnamese (Tiếng Việt)**
- **Admin Area**: All text in **English**
- **Font**: [Be Vietnam Pro](https://fonts.google.com/specimen/Be+Vietnam+Pro) for Vietnamese character support
- **Design**: Mobile-first responsive design

---

## 📁 Project Templates

The project integrates two pre-built HTML templates:

- **Public Area**: [KOPPEE](https://themewagon.com/themes/koppee-free-bootstrap-4-coffee-shop-html-template/) — Bootstrap 4 coffee shop template
- **Admin Area**: [MacAdmin](https://wrapbootstrap.com/theme/macadmin-responsive-admin-template) — Bootstrap 3 admin dashboard template

---

## 📄 License

This project is licensed under the MIT License.