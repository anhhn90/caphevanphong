# Technical Reference — Cà Phê Văn Phòng

See [CLAUDE.md](./CLAUDE.md) for project overview, roles, and business rules.
See [README.md](../README.md) for solution structure, layer responsibilities, tech stack, and CLI commands.

---

## Logging (Serilog)

Serilog is configured in `Program.cs` via `builder.Host.UseSerilog(...)`. Settings are read from `appsettings.json`.

### Sinks
| Sink | Level | Notes |
|------|-------|-------|
| Console | Information+ (Debug+ in dev) | Structured text output |
| File | Information+ | Rolling daily, 30-day retention — `logs/log-YYYYMMDD.txt` |
| Email | Error+ | Only active when `Email:SmtpPassword` is set (skipped in dev) |

### Email Configuration (`appsettings.json`)
```json
{
  "Email": {
    "ApplicationEmail": "nie.farm.coltd@gmail.com",
    "AdminEmail":       "anhngochoang.it@gmail.com",
    "SmtpHost":         "smtp.gmail.com",
    "SmtpPort":         "587",
    "SmtpUsername":     "nie.farm.coltd@gmail.com",
    "SmtpPassword":     ""
  }
}
```
- Set `SmtpPassword` to a **Gmail App Password** (not your account password) — requires Gmail 2-Step Verification.
- Leave `SmtpPassword` empty in development to disable the email sink.
- In production, set it via the Windows environment variable **`Cpvp_Email_SmtpPassword`** (configured in the hosting control panel). Do not commit the password to `appsettings.json`.
  - Why a custom name: shared Windows hosting panels often reject `__` (double-underscore), so the standard ASP.NET Core `Email__SmtpPassword` convention is avoided. `Program.cs` reads this variable explicitly and injects it into `builder.Configuration["Email:SmtpPassword"]`.

### Using the Logger
Inject `ILogger<T>` anywhere — Serilog is wired as the underlying provider:
```csharp
public class MyService(ILogger<MyService> logger)
{
    public void DoWork()
    {
        logger.LogInformation("Work started");
        logger.LogError(ex, "Something failed");   // triggers email in production
    }
}
```

### Packages
| Package | Version | Purpose |
|---------|---------|---------|
| `Serilog.AspNetCore` | 10.0.0 | Core + ILogger bridge + Console sink |
| `Serilog.Sinks.File` | 7.0.0 | Rolling file sink |
| `Serilog.Sinks.Email` | 4.1.0 | SMTP email sink (MailKit-based) |

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
- **DbContext:** `AppDbContext` in Infrastructure layer
- **Migrations:** Stored in `src/Infrastructure/CapheVanPhong.Infrastructure/Persistence/Migrations/`
- **Seed:** Default roles and the SuperAdmin account are seeded at startup via `DatabaseSeeder`

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
| `index.html` | Home page (carousel, about, services, product preview, testimonials) |
| `about.html` | About page |
| `menu.html` | Product catalog listing |
| `service.html` | Services page |
| `reservation.html` | Contact/inquiry page |
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
| Product/menu item | `<div class="row align-items-center mb-5">` with `.menu-price` |
| Footer | `<div class="container-fluid footer text-white ... overlay-top">` |
| Back to top | `<a href="#" class="btn btn-lg btn-primary btn-lg-square back-to-top">` |
| Primary color | Brown/coffee tone — defined in `style.css` as `--primary` |

### Blazor Integration Notes
- Owl Carousel and TempusDominus must be initialized via `IJSRuntime` in `OnAfterRenderAsync`
- Replace static `<a href="...html">` links with Blazor `<NavLink href="...">` components

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
