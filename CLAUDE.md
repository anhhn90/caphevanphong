# Project Instructions for Claude Code

## Project Overview
This is a **Blazor Server-Side Rendering (SSR)** web application built with **.NET**, following **Clean Architecture** principles, using **Entity Framework Core** with **MSSQL** as the database, and the **Repository Pattern** for data access abstraction.

The application is a Vietnamese café website (**Cà Phê Văn Phòng**) with two distinct areas:
- **Public Area** — accessible to all visitors; authenticated users with the `user` role can view and manage their own orders
- **Admin Area** — restricted to users with the `admin` role for full management of the café (menu, orders, users, etc.)

---

## Technology Stack
- **Framework:** Blazor Server-Side Rendering (.NET 8+)
- **Architecture:** Clean Architecture
- **ORM:** Entity Framework Core
- **Database:** Microsoft SQL Server (MSSQL)
- **Pattern:** Repository Pattern + Unit of Work
- **Authentication & Authorization:** ASP.NET Core Identity with Role-Based Access Control (RBAC)
- **UI:** Mobile-first responsive design (CSS / Bootstrap or Tailwind CSS)
- **Localization:** Vietnamese (Tiếng Việt) — primary language for the **Public area**; font must support Vietnamese characters. The **Admin area** uses **English** exclusively.

---

## Solution Structure

```
caphevanphong/
├── src/
│   ├── Core/
│   │   ├── CapheVanPhong.Domain/          # Entities, Value Objects, Domain Events, Interfaces
│   │   └── CapheVanPhong.Application/     # Use Cases, DTOs, Interfaces, Services, Validators
│   ├── Infrastructure/
│   │   └── CapheVanPhong.Infrastructure/  # EF Core DbContext, Repositories, Migrations, Identity, External Services
│   └── Presentation/
│       └── CapheVanPhong.Web/
│           ├── Components/
│           │   ├── Pages/
│           │   │   ├── Public/            # Public-facing pages (menu, home, order tracking)
│           │   │   └── Admin/             # Admin-only pages (dashboard, management)
│           │   ├── Shared/                # Shared/reusable components
│           │   └── Account/               # Login, Register, Logout pages
│           ├── Layout/
│           │   ├── PublicLayout.razor     # Layout for public area
│           │   └── AdminLayout.razor      # Layout for admin area
│           └── wwwroot/
│               └── css/                   # Global styles (mobile-first, Vietnamese font)
└── tests/
    ├── CapheVanPhong.Domain.Tests/
    ├── CapheVanPhong.Application.Tests/
    └── CapheVanPhong.Infrastructure.Tests/
```

---

## Layer Responsibilities

### Domain Layer (`CapheVanPhong.Domain`)
- Contains **Entities**, **Value Objects**, **Enums**, **Domain Events**, and **Domain Exceptions**
- Defines **repository interfaces** (e.g., `IRepository<T>`, `IUnitOfWork`)
- No dependencies on other layers or external packages (except `MediatR.Contracts` if using domain events)

### Application Layer (`CapheVanPhong.Application`)
- Contains **Use Cases** (Commands/Queries using CQRS with MediatR)
- Contains **DTOs**, **Validators** (FluentValidation), and **manual mapping methods** (no AutoMapper — map entities to DTOs by hand)
- Depends only on the **Domain** layer
- Defines interfaces for infrastructure services (e.g., `IEmailService`)

### Infrastructure Layer (`CapheVanPhong.Infrastructure`)
- Implements **EF Core DbContext** (`AppDbContext`)
- Implements **Repository Pattern** and **Unit of Work**
- Contains **EF Core Migrations**
- Implements interfaces defined in Application/Domain layers
- Depends on **Domain** and **Application** layers

### Presentation Layer (`CapheVanPhong.Web`)
- **Blazor Server-Side Rendering** components and pages
- Registers all services via Dependency Injection
- Depends on **Application** and **Infrastructure** layers (for DI registration only)
- Handles **authentication UI** (login/logout/register) via ASP.NET Core Identity scaffolding
- Enforces **route-level authorization** using `[Authorize(Roles = "admin")]` or `<AuthorizeView>`

---

## Areas & Authorization

### Roles
| Role | Description |
|---|---|
| `SuperAdmin` | Highest privilege; can do everything `admin` can, **plus** create/edit/delete `SuperAdmin` accounts. Seeded on startup via `DatabaseSeeder`. |
| `Admin` | Full access to the Admin area; can manage menu items, orders, and users — but **cannot** create, edit, or delete `SuperAdmin` accounts |
| `User` | Authenticated customer; can view and edit their own orders in the Public area |
| *(anonymous)* | Can browse the public menu and place orders (if allowed) |

### SuperAdmin Restrictions
Only `SuperAdmin` can:
- Create a user with the `SuperAdmin` role
- Edit or reset the password of a `SuperAdmin` user
- Delete a `SuperAdmin` user

These restrictions are enforced both in the UI (buttons/dropdowns hidden) and server-side (handler guards return early with an error).

### Route Protection
- Admin pages must be decorated with `@attribute [Authorize(Roles = "admin,SuperAdmin")]`
- User-specific pages (e.g., order history) must use `@attribute [Authorize(Roles = "user,admin,SuperAdmin")]`
- Use `<AuthorizeView Roles="admin,SuperAdmin">` in components to conditionally show UI elements
- Redirect unauthorized users to `/account/login` (configure in `Program.cs`)
- Use `HttpContext?.User.IsInRole("SuperAdmin")` to check SuperAdmin status within components

### Identity Setup
- Use **ASP.NET Core Identity** integrated with EF Core (`IdentityDbContext`)
- Seed default roles (`SuperAdmin`, `Admin`, `User`) in the `InitialCreate` migration via `InsertData`
- Seed the default SuperAdmin account at runtime via `DatabaseSeeder` (password hashing requires runtime execution)
- Store Identity tables in the same MSSQL database

---

## UI & Responsive Design

### Mobile-First
- All pages must be fully responsive and usable on mobile devices
- The AdminTemplate already uses **Bootstrap 3** with a responsive grid — leverage it fully
- Test layouts at breakpoints: 375px (mobile), 768px (tablet), 1280px (desktop)
- Navigation must collapse into a hamburger menu on mobile (already implemented in the template via `.navbar-toggle`)

### Vietnamese Font Support
- Use a Google Font that fully supports Vietnamese characters, such as:
  - **Be Vietnam Pro** (recommended)
  - **Nunito** 
  - **Inter**
- Import the font in `wwwroot/css/app.css` and override the template's default font:
  ```css
  @import url('https://fonts.googleapis.com/css2?family=Be+Vietnam+Pro:wght@300;400;500;600;700&display=swap');

  body, h1, h2, h3, h4, h5, h6, p, a, input, button, select, textarea {
      font-family: 'Be Vietnam Pro', sans-serif !important;
  }
  ```
- Ensure all text content, labels, and messages in the **Public area** are written in **Vietnamese (Tiếng Việt)**
- All text content, labels, and messages in the **Admin area** must be written in **English**

---

## Public Area Template Integration

### Overview
The Public area **must** use the existing static HTML/CSS/JS design located in the `PublicTemplate/` folder. This is the **KOPPEE** Bootstrap 4 coffee shop template. Do **not** replace it with a different CSS framework.

### Template Pages
| File | Purpose |
|---|---|
| `index.html` | Home page (carousel, about, services, menu preview, reservation, testimonials) |
| `about.html` | About page |
| `menu.html` | Full menu listing |
| `service.html` | Services page |
| `reservation.html` | Table reservation page |
| `testimonial.html` | Customer testimonials |
| `contact.html` | Contact page |

### Template Structure
The template uses Bootstrap 4 with the following layout pattern (replicate in `PublicLayout.razor`):
```
<body>
  <!-- Navbar (transparent, fixed-top) -->
  <div class="container-fluid p-0 nav-bar">
    <nav class="navbar navbar-expand-lg bg-none navbar-dark py-3">...</nav>
  </div>

  <!-- Page-specific content -->
  @Body

  <!-- Footer -->
  <div class="container-fluid footer text-white mt-5 pt-5 px-0 position-relative overlay-top">...</div>

  <!-- Back to Top -->
  <a href="#" class="btn btn-lg btn-primary btn-lg-square back-to-top">...</a>

  <!-- JS at bottom -->
</body>
```

### CSS Files to Include
Copy `PublicTemplate/css/` to `wwwroot/public/css/` and reference in `PublicLayout.razor`:
```html
<link href="https://fonts.googleapis.com/css2?family=Montserrat:wght@200;400&family=Roboto:wght@400;500;700&display=swap" rel="stylesheet">
<link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.10.0/css/all.min.css" rel="stylesheet">
<link href="~/public/lib/owlcarousel/assets/owl.carousel.min.css" rel="stylesheet">
<link href="~/public/lib/tempusdominus/css/tempusdominus-bootstrap-4.min.css" rel="stylesheet">
<link href="~/public/css/style.min.css" rel="stylesheet">
```

### JS Files to Include
Copy `PublicTemplate/js/` and `PublicTemplate/lib/` to `wwwroot/public/js/` and `wwwroot/public/lib/` respectively. Reference at the **bottom** of `PublicLayout.razor`:
```html
<script src="https://code.jquery.com/jquery-3.4.1.min.js"></script>
<script src="https://stackpath.bootstrapcdn.com/bootstrap/4.4.1/js/bootstrap.bundle.min.js"></script>
<script src="~/public/lib/easing/easing.min.js"></script>
<script src="~/public/lib/waypoints/waypoints.min.js"></script>
<script src="~/public/lib/owlcarousel/owl.carousel.min.js"></script>
<script src="~/public/lib/tempusdominus/js/moment.min.js"></script>
<script src="~/public/lib/tempusdominus/js/moment-timezone.min.js"></script>
<script src="~/public/lib/tempusdominus/js/tempusdominus-bootstrap-4.min.js"></script>
<script src="~/public/js/main.js"></script>
```

### Images
- Copy `PublicTemplate/img/` → `wwwroot/public/img/`

### Key Template CSS Classes & Patterns
| Element | Class/Pattern |
|---|---|
| Section title | `<div class="section-title"><h4 class="text-primary text-uppercase">` |
| Navbar | `<nav class="navbar navbar-expand-lg bg-none navbar-dark py-3">` |
| Hero carousel | `<div id="blog-carousel" class="carousel slide overlay-bottom">` |
| Offer/promo banner | `<div class="offer container-fluid ... overlay-top overlay-bottom">` |
| Reservation section | `<div class="reservation position-relative overlay-top overlay-bottom">` |
| Testimonial carousel | `<div class="owl-carousel testimonial-carousel">` |
| Menu item | `<div class="row align-items-center mb-5">` with `.menu-price` |
| Footer | `<div class="container-fluid footer text-white ... overlay-top">` |
| Back to top | `<a href="#" class="btn btn-lg btn-primary btn-lg-square back-to-top">` |
| Primary color | Brown/coffee tone — defined in `style.css` as `--primary` |

### Blazor Integration Notes
- The Owl Carousel (testimonials) and TempusDominus (date/time pickers) must be initialized via `IJSRuntime` in `OnAfterRenderAsync`
- The Waypoints library triggers scroll animations — re-initialize on Blazor navigation
- The navbar collapse is handled by Bootstrap 4's built-in JS (included via `bootstrap.bundle.min.js`)
- Replace static `<a href="...html">` links with Blazor `<NavLink href="...">` components
- The reservation form should be wired to a Blazor `EditForm` with model binding instead of the static HTML form

### File Placement
```
wwwroot/
├── public/
│   ├── css/        ← copied from PublicTemplate/css/
│   ├── js/         ← copied from PublicTemplate/js/
│   ├── lib/        ← copied from PublicTemplate/lib/
│   └── img/        ← copied from PublicTemplate/img/
```

---

## Admin Area Template Integration

### Overview
The Admin area **must** use the existing static HTML/CSS/JS design located in the `AdminTemplate/` folder. This is the **MacAdmin** Bootstrap 3 template. Do **not** replace it with a different CSS framework.

### Template Structure
The template uses the following layout pattern (replicate this in `AdminLayout.razor`):
```
<body>
  <!-- Top navbar (fixed) -->
  <div class="navbar navbar-fixed-top bs-docs-nav">...</div>

  <!-- Header with logo + quick stats -->
  <header>...</header>

  <!-- Main content area -->
  <div class="content">
    <!-- Left sidebar with navigation -->
    <div class="sidebar">
      <ul id="nav">...</ul>
    </div>

    <!-- Main content -->
    <div class="mainbar">
      <div class="page-head">...</div>  <!-- Page title + breadcrumb -->
      <div class="matter">
        <div class="container">
          <!-- Page content goes here -->
        </div>
      </div>
    </div>
  </div>

  <!-- Footer -->
  <footer>...</footer>
</body>
```

### CSS Files to Include (from `AdminTemplate/css/`)
Copy all files from `AdminTemplate/css/` to `wwwroot/admin/css/` and reference them in `AdminLayout.razor`:
```html
<link href="~/admin/css/bootstrap.min.css" rel="stylesheet">
<link rel="stylesheet" href="~/admin/css/font-awesome.min.css">
<link rel="stylesheet" href="~/admin/css/jquery-ui.css">
<link rel="stylesheet" href="~/admin/css/fullcalendar.css">
<link rel="stylesheet" href="~/admin/css/prettyPhoto.css">
<link rel="stylesheet" href="~/admin/css/rateit.css">
<link rel="stylesheet" href="~/admin/css/bootstrap-datetimepicker.min.css">
<link rel="stylesheet" href="~/admin/css/jquery.cleditor.css">
<link rel="stylesheet" href="~/admin/css/jquery.dataTables.css">
<link rel="stylesheet" href="~/admin/css/jquery.onoff.css">
<link href="~/admin/css/style.css" rel="stylesheet">
<link href="~/admin/css/widgets.css" rel="stylesheet">
```

### JS Files to Include (from `AdminTemplate/js/`)
Copy all files from `AdminTemplate/js/` to `wwwroot/admin/js/` and reference them at the **bottom** of `AdminLayout.razor` (before `</body>`):
```html
<script src="~/admin/js/jquery.js"></script>
<script src="~/admin/js/bootstrap.min.js"></script>
<script src="~/admin/js/jquery-ui.min.js"></script>
<script src="~/admin/js/fullcalendar.min.js"></script>
<script src="~/admin/js/jquery.rateit.min.js"></script>
<script src="~/admin/js/jquery.prettyPhoto.js"></script>
<script src="~/admin/js/jquery.slimscroll.min.js"></script>
<script src="~/admin/js/jquery.dataTables.min.js"></script>
<script src="~/admin/js/excanvas.min.js"></script>
<script src="~/admin/js/jquery.flot.js"></script>
<script src="~/admin/js/jquery.flot.resize.js"></script>
<script src="~/admin/js/jquery.flot.pie.js"></script>
<script src="~/admin/js/jquery.flot.stack.js"></script>
<script src="~/admin/js/jquery.noty.js"></script>
<script src="~/admin/js/themes/default.js"></script>
<script src="~/admin/js/layouts/bottom.js"></script>
<script src="~/admin/js/layouts/topRight.js"></script>
<script src="~/admin/js/layouts/top.js"></script>
<script src="~/admin/js/sparklines.js"></script>
<script src="~/admin/js/jquery.cleditor.min.js"></script>
<script src="~/admin/js/bootstrap-datetimepicker.min.js"></script>
<script src="~/admin/js/jquery.onoff.min.js"></script>
<script src="~/admin/js/filter.js"></script>
<script src="~/admin/js/custom.js"></script>
<script src="~/admin/js/charts.js"></script>
```

### Images & Fonts
- Copy `AdminTemplate/img/` → `wwwroot/admin/img/`
- Copy `AdminTemplate/fonts/` → `wwwroot/admin/fonts/`
- Copy `AdminTemplate/images/` → `wwwroot/admin/images/`

### Key Template CSS Classes & Patterns
| Element | Class/Pattern |
|---|---|
| Page wrapper | `<div class="content">` |
| Sidebar | `<div class="sidebar"><ul id="nav">` |
| Main content | `<div class="mainbar">` |
| Page heading | `<div class="page-head">` |
| Content area | `<div class="matter">` |
| Widget card | `<div class="widget">` |
| Widget header | `<div class="widget-head">` |
| Widget body | `<div class="widget-content"><div class="padd">` |
| Widget footer | `<div class="widget-foot">` |
| Sidebar sub-menu | Add class `has_sub` to `<li>`, add `open` for expanded |
| Active menu item | Add class `current` to `<li>` |
| Colored widgets | `wblack`, `wred`, `wgreen`, `wblue`, `worange`, `wviolet` |
| Scroll to top | `<span class="totop"><a href="#"><i class="fa fa-chevron-up"></i></a></span>` |

### Blazor Integration Notes
- Since Blazor SSR uses server-side rendering, jQuery plugins (DataTables, FullCalendar, etc.) must be initialized via `IJSRuntime` after the component renders
- Use `@inject IJSRuntime JS` and call `await JS.InvokeVoidAsync(...)` in `OnAfterRenderAsync`
- The sidebar `custom.js` handles accordion navigation — ensure it runs after Blazor renders the DOM
- For Blazor navigation (no full page reload), re-initialize jQuery plugins on each navigation using `OnAfterRenderAsync(bool firstRender)`
- Use `<HeadContent>` in Blazor pages to add page-specific scripts if needed
- The template's `login.html` and `register.html` use a standalone layout (no sidebar) — use a separate `AdminLoginLayout.razor` for auth pages

### File Placement
```
wwwroot/
├── admin/
│   ├── css/        ← copied from AdminTemplate/css/
│   ├── js/         ← copied from AdminTemplate/js/
│   ├── img/        ← copied from AdminTemplate/img/
│   ├── fonts/      ← copied from AdminTemplate/fonts/
│   └── images/     ← copied from AdminTemplate/images/
└── css/
    └── app.css     ← global styles (Vietnamese font override)
```

---

## Coding Conventions

### General
- Use **C# latest stable** language features
- Follow **PascalCase** for classes, methods, and properties
- Follow **camelCase** for local variables and parameters
- Use **`var`** when the type is obvious from the right-hand side
- Prefer **`async/await`** for all I/O operations
- Use **nullable reference types** (`#nullable enable`)

### Entities
- All entities inherit from a `BaseEntity` class with `Id`, `CreatedAt`, `UpdatedAt`
- Use **private setters** and expose behavior through domain methods
- Example:
  ```csharp
  public class Product : BaseEntity
  {
      public string Name { get; private set; }
      public decimal Price { get; private set; }

      private Product() { } // EF Core constructor

      public static Product Create(string name, decimal price)
      {
          // validation logic
          return new Product { Name = name, Price = price };
      }
  }
  ```

### Repository Pattern
- Define generic interface in Domain:
  ```csharp
  public interface IRepository<T> where T : BaseEntity
  {
      Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
      Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
      Task AddAsync(T entity, CancellationToken cancellationToken = default);
      void Update(T entity);
      void Delete(T entity);
  }
  ```
- Define specific repository interfaces in Domain (e.g., `IProductRepository : IRepository<Product>`)
- Implement repositories in Infrastructure layer

### Unit of Work
- Define `IUnitOfWork` in Domain:
  ```csharp
  public interface IUnitOfWork
  {
      Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
  }
  ```
- Implement in Infrastructure using `AppDbContext`

### EF Core
- Use **Fluent API** for entity configuration (avoid Data Annotations on entities)
- Place configurations in separate `IEntityTypeConfiguration<T>` classes
- Use **migrations** for schema changes (`dotnet ef migrations add`)
- Connection string stored in `appsettings.json` under `ConnectionStrings:DefaultConnection`

### Dependency Injection
- Register services using **extension methods** per layer:
  - `services.AddApplication()` in Application layer
  - `services.AddInfrastructure(configuration)` in Infrastructure layer
- Register in `Program.cs` of the Web project

### Blazor Components
- Place pages in `Components/Pages/`
- Place shared/reusable components in `Components/Shared/`
- Use `@inject` for service injection in components
- Prefer **`@code` blocks** over code-behind files for simple components
- Use code-behind (`.razor.cs`) for complex components

---

## Database Configuration

- **Provider:** Microsoft SQL Server
- **Connection String Key:** `ConnectionStrings:DefaultConnection`
- **DbContext:** `AppDbContext` in Infrastructure layer
- **Migrations:** Stored in `Infrastructure/Persistence/Migrations/`

Example `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CapheVanPhong;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

---

## Key NuGet Packages

| Package | Layer |
|---|---|
| `Microsoft.EntityFrameworkCore.SqlServer` | Infrastructure |
| `Microsoft.EntityFrameworkCore.Tools` | Infrastructure |
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | Infrastructure |
| `MediatR` | Application |
| `FluentValidation` | Application |
| `Microsoft.AspNetCore.Components` | Web (built-in) |
| `Microsoft.AspNetCore.Components.Authorization` | Web (built-in) |

---

## Commands Reference

```bash
# Add EF Core migration
dotnet ef migrations add <MigrationName> --project src/Infrastructure/CapheVanPhong.Infrastructure --startup-project src/Presentation/CapheVanPhong.Web

# Apply migrations
dotnet ef database update --project src/Infrastructure/CapheVanPhong.Infrastructure --startup-project src/Presentation/CapheVanPhong.Web

# Run the application
dotnet run --project src/Presentation/CapheVanPhong.Web
```

---

## Important Rules
1. **Never** put business logic in Blazor components — delegate to Application layer services
2. **Never** reference Infrastructure layer directly from components (except DI registration in `Program.cs`)
3. **Always** use the Repository pattern — do not use `DbContext` directly outside of Infrastructure
4. **Always** use `CancellationToken` in async methods
5. **Always** validate inputs at the Application layer using FluentValidation
6. Keep **Domain layer** free of any framework dependencies
7. **Always** protect Admin pages with `[Authorize(Roles = "admin,")]` — never rely on UI-only hiding
8. **Always** scope user data queries to the currently logged-in user's ID (never expose other users' orders)
9. **Always** use a Vietnamese-compatible font; write all UI text in **Vietnamese** for the **Public area** and in **English** for the **Admin area**
10. **Always** design components mobile-first — start with the smallest screen size and scale up
