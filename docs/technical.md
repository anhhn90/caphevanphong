# Technical Reference — Cà Phê Văn Phòng

See [CLAUDE.md](../CLAUDE.md) for project overview, roles, and business rules.

---

## Solution Structure

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
- Contains **DTOs**, **Validators** (FluentValidation), and **manual mapping methods** (no AutoMapper)
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
- Handles **authentication UI** (login/logout/register) via ASP.NET Core Identity
- Enforces **route-level authorization** using `[Authorize(Roles = "Admin,SuperAdmin")]` or `<AuthorizeView>`

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

## UI & Responsive Design

### Mobile-First
- All pages must be fully responsive and usable on mobile devices
- The AdminTemplate already uses **Bootstrap 3** with a responsive grid — leverage it fully
- Test layouts at breakpoints: 375px (mobile), 768px (tablet), 1280px (desktop)
- Navigation must collapse into a hamburger menu on mobile (already implemented via `.navbar-toggle`)

### Vietnamese Font Support
- Use a Google Font that fully supports Vietnamese characters, such as **Be Vietnam Pro** (recommended)
- Import in `wwwroot/css/app.css` and override the template's default font:
  ```css
  @import url('https://fonts.googleapis.com/css2?family=Be+Vietnam+Pro:wght@300;400;500;600;700&display=swap');

  body, h1, h2, h3, h4, h5, h6, p, a, input, button, select, textarea {
      font-family: 'Be Vietnam Pro', sans-serif !important;
  }
  ```

---

## Public Area Template Integration

The Public area **must** use the existing static HTML/CSS/JS design from `PublicTemplate/` (**KOPPEE** Bootstrap 4 template). Do **not** replace it with a different CSS framework.

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

### Layout Pattern (replicate in `PublicLayout.razor`)
```html
<body>
  <div class="container-fluid p-0 nav-bar">
    <nav class="navbar navbar-expand-lg bg-none navbar-dark py-3">...</nav>
  </div>
  @Body
  <div class="container-fluid footer text-white mt-5 pt-5 px-0 position-relative overlay-top">...</div>
  <a href="#" class="btn btn-lg btn-primary btn-lg-square back-to-top">...</a>
</body>
```

### CSS Files (copy `PublicTemplate/css/` → `wwwroot/public/css/`)
```html
<link href="https://fonts.googleapis.com/css2?family=Montserrat:wght@200;400&family=Roboto:wght@400;500;700&display=swap" rel="stylesheet">
<link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.10.0/css/all.min.css" rel="stylesheet">
<link href="~/public/lib/owlcarousel/assets/owl.carousel.min.css" rel="stylesheet">
<link href="~/public/lib/tempusdominus/css/tempusdominus-bootstrap-4.min.css" rel="stylesheet">
<link href="~/public/css/style.min.css" rel="stylesheet">
```

### JS Files (bottom of `PublicLayout.razor`)
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

### Key CSS Classes
| Element | Class/Pattern |
|---|---|
| Section title | `<div class="section-title"><h4 class="text-primary text-uppercase">` |
| Navbar | `<nav class="navbar navbar-expand-lg bg-none navbar-dark py-3">` |
| Hero carousel | `<div id="blog-carousel" class="carousel slide overlay-bottom">` |
| Menu item | `<div class="row align-items-center mb-5">` with `.menu-price` |
| Footer | `<div class="container-fluid footer text-white ... overlay-top">` |
| Back to top | `<a href="#" class="btn btn-lg btn-primary btn-lg-square back-to-top">` |
| Primary color | Brown/coffee tone — defined in `style.css` as `--primary` |

### Blazor Integration Notes
- Owl Carousel and TempusDominus must be initialized via `IJSRuntime` in `OnAfterRenderAsync`
- Replace static `<a href="...html">` links with Blazor `<NavLink href="...">` components
- The reservation form should be a Blazor `EditForm` with model binding

### File Placement
```
wwwroot/public/
├── css/    ← PublicTemplate/css/
├── js/     ← PublicTemplate/js/
├── lib/    ← PublicTemplate/lib/
└── img/    ← PublicTemplate/img/
```

---

## Admin Area Template Integration

The Admin area **must** use the existing static HTML/CSS/JS design from `AdminTemplate/` (**MacAdmin** Bootstrap 3 template). Do **not** replace it with a different CSS framework.

### Layout Pattern (replicate in `AdminLayout.razor`)
```html
<body>
  <div class="navbar navbar-fixed-top bs-docs-nav">...</div>
  <header>...</header>
  <div class="content">
    <div class="sidebar"><ul id="nav">...</ul></div>
    <div class="mainbar">
      <div class="page-head">...</div>
      <div class="matter"><div class="container">@Body</div></div>
    </div>
  </div>
  <footer>...</footer>
</body>
```

### CSS Files (copy `AdminTemplate/css/` → `wwwroot/admin/css/`)
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

### JS Files (bottom of `AdminLayout.razor`)
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

### Key CSS Classes
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
| Sidebar expandable item | Add `has_sub` to `<li>`; add `open` when expanded |
| Top-level active item (no sub-menu) | Add `open` to `<li>` — triggers blue highlight + white caret |
| Active nested sub-menu item | Add `current` to `<li>` — **nested `<li>` only**; has no effect on top-level items |
| Colored widgets | `wblack`, `wred`, `wgreen`, `wblue`, `worange`, `wviolet` |
| Scroll to top | `<span class="totop"><a href="#"><i class="fa fa-chevron-up"></i></a></span>` |

### Blazor Integration Notes
- jQuery plugins (DataTables, FullCalendar, etc.) must be initialized via `IJSRuntime` in `OnAfterRenderAsync`
- The sidebar `custom.js` handles accordion navigation — ensure it runs after Blazor renders the DOM
- Re-initialize jQuery plugins on each Blazor navigation using `OnAfterRenderAsync(bool firstRender)`
- Use `<HeadContent>` in Blazor pages to add page-specific scripts
- Auth pages (login/register) use a standalone layout — use a separate `AdminLoginLayout.razor`

### File Placement
```
wwwroot/admin/
├── css/      ← AdminTemplate/css/
├── js/       ← AdminTemplate/js/
├── img/      ← AdminTemplate/img/
├── fonts/    ← AdminTemplate/fonts/
└── images/   ← AdminTemplate/images/
```
