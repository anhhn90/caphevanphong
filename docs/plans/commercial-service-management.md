# Commercial Service Management — Implementation Plan

## Overview

The `/dich-vu` public page renders hardcoded data. This plan makes only the **"Dịch Vụ Của Chúng Tôi"** section dynamic via a single `CommercialService` entity managed from the Admin area. The "Tại Sao Chọn Chúng Tôi" and "Từ Hạt Đến Tách" sections remain hardcoded.

Additional features:
- Each service can have optional rich-text **Content** → shows "Chi tiết..." link → detail page at `/dich-vu/{slug}`
- Navbar "Dịch Vụ" link becomes a hover dropdown showing active services (same pattern as "Tin Tức")
- The 6 existing hardcoded services are seeded into `cpvp_data_initializer.sql`

---

## Entity: `CommercialService` (single entity — no ServiceFeature)

| Field | Type | Notes |
|---|---|---|
| `Title` | string | required, max 200 |
| `Slug` | string | required, max 250, unique — auto-generated from Title |
| `Introduction` | nvarchar(max) | required, rich text — shown on overview card |
| `Content` | nvarchar(max)? | optional rich text — if not empty: show "Chi tiết..." + detail page |
| `IconClass` | string | required, max 100 |
| `ImageName` | string? | optional, max 255 |
| `IsActive` | bool | default true |
| `DisplayOrder` | int | default 0 |

---

## Layer-by-Layer Changes

### 1. Domain Layer — `src/Core/CapheVanPhong.Domain/`

**New files:**

- `Entities/CommercialService.cs`
  - Extends `BaseEntity`; private setters
  - `static Create(title, slug, introduction, content, iconClass, imageName, isActive, displayOrder)`
  - `void Update(...)`, `void SetActive(bool)`
- `Interfaces/ICommercialServiceRepository.cs`
  - Extends `IRepository<CommercialService>`
  - `GetActiveOrderedAsync(ct)` → `Task<IReadOnlyList<CommercialService>>`
  - `GetBySlugAsync(slug, ct)` → `Task<CommercialService?>`
  - `SlugExistsAsync(slug, excludeId?, ct)` → `Task<bool>`

---

### 2. Infrastructure Layer — `src/Infrastructure/CapheVanPhong.Infrastructure/`

**New files:**

- `Persistence/Configurations/CommercialServiceConfiguration.cs`
  - Table: `CommercialServices`; unique index on `Slug`
  - `Introduction` required `nvarchar(max)`; `Content` nullable `nvarchar(max)`
  - `IsActive` default true; `DisplayOrder` default 0
- `Persistence/Repositories/CommercialServiceRepository.cs`
  - `GetActiveOrderedAsync`: `.Where(s => s.IsActive).OrderBy(s => s.DisplayOrder)`
  - `GetBySlugAsync`, `SlugExistsAsync`

**Modified files:**

- `Persistence/AppDbContext.cs` — add `DbSet<CommercialService> CommercialServices`
- `DependencyInjection.cs` — `services.AddScoped<ICommercialServiceRepository, CommercialServiceRepository>()`

> ⚠️ **Developer action required:** add a new EF Core migration after implementation.

---

### 3. Application Layer — `src/Core/CapheVanPhong.Application/`

**New files:**

- `Services/ICommercialServiceService.cs`
  ```
  GetAllAsync(ct)
  GetActiveAsync(ct)
  GetByIdAsync(id, ct)
  GetBySlugAsync(slug, ct)
  CreateAsync(title, introduction, content, iconClass, isActive, displayOrder, imageStream?, imageFileName?, ct) → (bool, string?)
  UpdateAsync(id, title, introduction, content, iconClass, isActive, displayOrder, existingImageName?, newImageStream?, newImageFileName?, ct) → (bool, string?)
  DeleteAsync(id, ct) → (bool, string?)
  ```
- `Services/CommercialServiceService.cs`
  - Inject `ICommercialServiceRepository`, `IUnitOfWork`, `IFileStorageService`
  - Image subfolder: `"commercial-services"`
  - Slug via `SlugHelper.Generate(title)`, uniqueness checked — follow `BlogService.cs` pattern exactly

**Modified files:**

- `DependencyInjection.cs` — `services.AddScoped<ICommercialServiceService, CommercialServiceService>()`

---

### 4. Admin Presentation Layer — `src/Presentation/CapheVanPhong.Web/Components/Pages/Admin/`

Follow `Blogs/` pattern. CLEditor for **Introduction** + **Content** (same as Blog Create/Edit).

**New files:**

| File | Route | Description |
|---|---|---|
| `CommercialServices/Index.razor` | `/admin/commercial-services` | List: ID, Title, Slug, Image, Has Content, Status, DisplayOrder, Actions; delete modal |
| `CommercialServices/Create.razor` | `/admin/commercial-services/create` | Title→auto-Slug, Introduction (CLEditor required), Content (CLEditor optional — label: "leave empty for no detail page"), IconClass + `<i>` preview, DisplayOrder, IsActive, image ≤5 MB |
| `CommercialServices/Edit.razor` | `/admin/commercial-services/edit/{Id:int}` | Pre-populated; image preview + "Remove image" checkbox |

All pages: `@rendermode InteractiveServer` (Create/Edit), `[Authorize(Roles = "Admin,SuperAdmin")]`.

**Modified files:**

- Admin sidebar component (find under `Components/Shared/Admin/`) — add `/admin/commercial-services` link

---

### 5. Public Presentation Layer

**New file:**

- `src/.../Pages/Public/ChiTietDichVu.razor`
  - Route: `/dich-vu/{Slug}`
  - Shows image (if any), icon, title, introduction, `@((MarkupString)_service.Content)`
  - 404 state if slug missing or service inactive
  - Breadcrumb: Trang Chủ / Dịch Vụ / [Title]
  - Back link: "← Quay lại Dịch Vụ" → `/dich-vu`

**Modified files:**

- `src/.../Pages/Public/Service.razor.cs` (ServiceBase)
  - Inject `ICommercialServiceService`
  - `LoadServicesAsync()`: call `GetActiveAsync()`, map to updated `ServiceViewModel` record (add `Slug`, `HasContent` fields)
  - `ImageUrl` = `/public/img/commercial-services/{imageName}` (empty string if null)
  - Remove hardcoded services; keep `LoadFeaturesAsync()` hardcoded unchanged

- `src/.../Pages/Public/Service.razor`
  - In the services `foreach` loop, below `<p class="m-0">@service.Description</p>`, add:
    ```html
    @if (service.HasContent)
    {
        <a href="/dich-vu/@service.Slug" class="btn btn-sm btn-outline-primary mt-2">Chi tiết...</a>
    }
    ```

- `src/.../Components/Shared/Public/Navbar.razor`
  - Replace `<NavLink href="/dich-vu" ...>Dịch Vụ</NavLink>` with a dropdown following the "Tin Tức" pattern
  - Load active services using `ICommercialServiceService` via `IServiceScopeFactory` (same pattern as `_blogCategories`)
  - Items link to `/dich-vu/{service.Slug}`; trigger `<a>` links to `/dich-vu` (overview)
  - Empty fallback: `<span class="dropdown-item text-muted disabled">Sắp có...</span>`

---

## Key Reuse References

| Pattern | Reference |
|---|---|
| Domain entity + slug | `src/Core/.../Entities/Blog.cs` |
| Repository with slug lookup | `src/Core/.../Interfaces/IBlogRepository.cs` |
| EF config + unique slug index | `src/Infrastructure/.../Configurations/BlogConfiguration.cs` |
| Service with image + slug lifecycle | `src/Core/.../Services/BlogService.cs` |
| Admin Create/Edit with CLEditor | `src/Presentation/.../Pages/Admin/Blogs/Create.razor` |
| Navbar dropdown + scoped service | `src/Presentation/.../Components/Shared/Public/Navbar.razor` |
| Public detail page | `src/Presentation/.../Pages/Public/TinTucChiTiet.razor` |

---

## Files Modified Summary

| File | Change |
|---|---|
| `AppDbContext.cs` | Add 1 DbSet |
| `Infrastructure/DependencyInjection.cs` | Register 1 repository |
| `Application/DependencyInjection.cs` | Register 1 service |
| `Service.razor.cs` | Replace hardcoded services; keep hardcoded features |
| `Service.razor` | Add conditional "Chi tiết..." link |
| `Navbar.razor` | "Dịch Vụ" → hover dropdown |
| Admin sidebar | Add nav link |
| `cpvp_data_initializer.sql` | Append 6 INSERT statements |

---

## Verification

1. Apply migration → run app → `/dich-vu` renders (empty services, no error)
2. Run SQL seed → 6 services appear with images and icons
3. Hover "Dịch Vụ" in navbar → dropdown lists 6 services
4. Click navbar service → detail page at `/dich-vu/{slug}` loads
5. Admin → `/admin/commercial-services`: list / create with CLEditor / edit / delete all work
6. Create service with Content → "Chi tiết..." appears on card; detail page shows content
7. Create service without Content → no "Chi tiết..." shown
8. Deactivate → disappears from public page and navbar dropdown
9. Delete → removed; image soft-deleted on disk

> ⚠️ **Developer action required:** add new EF Core migration for `CommercialServices` table.
