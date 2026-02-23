# Category Management — Implementation Plan

## Overview

Implement a full Category hierarchy for the Cà Phê Văn Phòng e-commerce site, modeled after the mega-menu navigation shown in the UI screenshot. The system allows admins to CRUD categories, sub-category groups, sub-categories, and brands, and enables product filtering by these dimensions.

---

## Hierarchy (from the screenshot)

```
Category (main — left panel)
│  e.g. "Máy Pha Cà Phê"
│
├── SubCategoryGroup (mega-menu columns)
│     e.g. "Phân Loại", "Nhu Cầu", "Group Heads"
│     Each group belongs to ONE Category
│
└── SubCategory (items inside each column)
      e.g. "Máy Pha Cà Phê Espresso"
      Each SubCategory belongs to ONE Category
      Each SubCategory belongs to ONE SubCategoryGroup (1:N)

Brand (independent — "Thương Hiệu" column)
  e.g. "Foresto", "Hambach", "La Marzocco"
  Products are tagged with a Brand
```

---

## Entity Design

### `Category` (updated)
| Field | Type | Notes |
|-------|------|-------|
| Id | int | PK |
| Name | string(200) | Required |
| Slug | string(200) | Unique, SEO-friendly |
| Description | string(1000) | |
| ImageUrl | string(500) | Nullable |
| DisplayOrder | int | For ordering in nav |
| IsActive | bool | Show/hide in nav |
| CreatedAt, UpdatedAt | DateTime | From BaseEntity |

Navigation: → SubCategories, → SubCategoryGroups, → Products

### `SubCategoryGroup` (new)
| Field | Type | Notes |
|-------|------|-------|
| Id | int | PK |
| Name | string(200) | e.g. "Phân Loại" |
| DisplayOrder | int | Column order in mega-menu |
| IsActive | bool | |
| CategoryId | int | FK → Category |

Navigation: → Category, → SubCategories (1:N)

### `SubCategory` (new)
| Field | Type | Notes |
|-------|------|-------|
| Id | int | PK |
| Name | string(200) | e.g. "Máy Pha Cà Phê Espresso" |
| Slug | string(200) | Unique |
| Description | string(1000) | Nullable |
| ImageUrl | string(500) | Nullable |
| DisplayOrder | int | |
| IsActive | bool | |
| CategoryId | int | FK → Category |
| SubCategoryGroupId | int? | FK → SubCategoryGroup, nullable |

Navigation: → Category, → SubCategoryGroup, → Products

### `Brand` (new — "Thương Hiệu")
| Field | Type | Notes |
|-------|------|-------|
| Id | int | PK |
| Name | string(200) | e.g. "La Marzocco" |
| Slug | string(200) | Unique |
| Description | string(1000) | Nullable |
| LogoUrl | string(500) | Nullable |
| DisplayOrder | int | |
| IsActive | bool | |

Navigation: → Products

### `Product` (updated)
Added fields:
- `SubCategoryId int?` — FK to SubCategory
- `BrandId int?` — FK to Brand

---

## Domain Interfaces

```csharp
// ICategoryRepository.cs
public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<IReadOnlyList<Category>> GetAllWithSubCategoriesAsync(CancellationToken ct = default);
    Task<bool> SlugExistsAsync(string slug, int? excludeId = null, CancellationToken ct = default);
}

// ISubCategoryRepository.cs
public interface ISubCategoryRepository : IRepository<SubCategory>
{
    Task<SubCategory?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<IReadOnlyList<SubCategory>> GetByCategoryIdAsync(int categoryId, CancellationToken ct = default);
    Task<bool> SlugExistsAsync(string slug, int? excludeId = null, CancellationToken ct = default);
}

// ISubCategoryGroupRepository.cs
public interface ISubCategoryGroupRepository : IRepository<SubCategoryGroup>
{
    Task<IReadOnlyList<SubCategoryGroup>> GetByCategoryIdAsync(int categoryId, CancellationToken ct = default);
    Task<IReadOnlyList<SubCategoryGroup>> GetByCategoryIdWithSubCategoriesAsync(int categoryId, CancellationToken ct = default);
}

// IBrandRepository.cs
public interface IBrandRepository : IRepository<Brand>
{
    Task<Brand?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<bool> SlugExistsAsync(string slug, int? excludeId = null, CancellationToken ct = default);
}
```

---

## Application Services

### `SlugHelper`
Converts Vietnamese text to URL-safe slugs:
- "Máy Pha Cà Phê" → `may-pha-ca-phe`
- Strips diacritics, lowercases, replaces spaces with `-`, removes special chars

### Service Interfaces & Implementations

| Interface | Implementation | Operations |
|-----------|---------------|------------|
| `ICategoryService` | `CategoryService` | GetAll, GetById, GetBySlug, Create, Update, Delete |
| `ISubCategoryService` | `SubCategoryService` | GetAll, GetByCategoryId, GetById, Create, Update, Delete |
| `ISubCategoryGroupService` | `SubCategoryGroupService` | GetByCategoryId, GetById, Create, Update, Delete |
| `IBrandService` | `BrandService` | GetAll, GetById, GetBySlug, Create, Update, Delete |

---

## Infrastructure

### EF Configurations (new/updated)
- `CategoryConfiguration` — add Slug (unique index), DisplayOrder, IsActive, nav to SubCategories + Groups
- `SubCategoryConfiguration` — map entity + SubCategoryGroupId FK (nullable)
- `SubCategoryGroupConfiguration` — map entity + nav to SubCategories
- `BrandConfiguration` — map entity
- `ProductConfiguration` — add SubCategoryId (nullable FK), BrandId (nullable FK)

### Repositories (new)
- `CategoryRepository` — implements `ICategoryRepository`
- `SubCategoryRepository` — implements `ISubCategoryRepository`
- `SubCategoryGroupRepository` — implements `ISubCategoryGroupRepository`
- `BrandRepository` — implements `IBrandRepository`

### Migration
Name: `AddCategoryHierarchy`

Changes:
- Add `Slug`, `DisplayOrder`, `IsActive` columns to `Categories`
- Create `SubCategoryGroups` table
- Create `SubCategories` table (with SubCategoryGroupId nullable FK)
- Create `Brands` table
- Add `SubCategoryId`, `BrandId` nullable FK columns to `Products`

Run command:
```bash
dotnet ef migrations add AddCategoryHierarchy \
  --project src/Infrastructure/CapheVanPhong.Infrastructure \
  --startup-project src/Presentation/CapheVanPhong.Web
```

---

## Admin Pages (Blazor SSR)

All pages use `AdminLayout`, Bootstrap 3, standard `@formname` POST pattern.

| Page | Route | Description |
|------|-------|-------------|
| Category List | `/admin/categories` | Table with Name, Slug, SubCategory count, IsActive, actions |
| Create Category | `/admin/categories/create` | Form: Name, Slug (auto-gen), Description, ImageUrl, DisplayOrder, IsActive |
| Edit Category | `/admin/categories/edit/{id}` | Same form, pre-populated |
| SubCategory List | `/admin/categories/{categoryId}/subcategories` | Table scoped to one Category |
| Create SubCategory | `/admin/categories/{categoryId}/subcategories/create` | Form + group assignment |
| Edit SubCategory | `/admin/categories/{categoryId}/subcategories/edit/{id}` | Same |
| SubCategoryGroup List | `/admin/categories/{categoryId}/groups` | Table scoped to one Category |
| Create SubCategoryGroup | `/admin/categories/{categoryId}/groups/create` | Form: Name, Slug, DisplayOrder, IsActive |
| Edit SubCategoryGroup | `/admin/categories/{categoryId}/groups/edit/{id}` | Same |
| Brand List | `/admin/brands` | Table with Name, Slug, IsActive, Product count |
| Create Brand | `/admin/brands/create` | Form: Name, Slug, Description, LogoUrl, DisplayOrder, IsActive |
| Edit Brand | `/admin/brands/edit/{id}` | Same |

### AdminSidebar Update
Add new expandable menu items:
- **Danh Mục** (`/admin/categories`) — with sub: Sub Categories, Groups
- **Thương Hiệu** (`/admin/brands`)

---

## Files Created / Modified

### Domain (`CapheVanPhong.Domain`)
| File | Action |
|------|--------|
| `Entities/Category.cs` | Update — add Slug, DisplayOrder, IsActive, nav props |
| `Entities/SubCategory.cs` | **New** |
| `Entities/SubCategoryGroup.cs` | **New** |
| `Entities/Brand.cs` | **New** |
| `Entities/Product.cs` | Update — add SubCategoryId, BrandId |
| `Interfaces/ICategoryRepository.cs` | **New** |
| `Interfaces/ISubCategoryRepository.cs` | **New** |
| `Interfaces/ISubCategoryGroupRepository.cs` | **New** |
| `Interfaces/IBrandRepository.cs` | **New** |

### Application (`CapheVanPhong.Application`)
| File | Action |
|------|--------|
| `Helpers/SlugHelper.cs` | **New** |
| `Services/ICategoryService.cs` | **New** |
| `Services/CategoryService.cs` | **New** |
| `Services/ISubCategoryService.cs` | **New** |
| `Services/SubCategoryService.cs` | **New** |
| `Services/ISubCategoryGroupService.cs` | **New** |
| `Services/SubCategoryGroupService.cs` | **New** |
| `Services/IBrandService.cs` | **New** |
| `Services/BrandService.cs` | **New** |
| `DependencyInjection.cs` | Update — register services |

### Infrastructure (`CapheVanPhong.Infrastructure`)
| File | Action |
|------|--------|
| `Persistence/AppDbContext.cs` | Update — add DbSets |
| `Persistence/Configurations/CategoryConfiguration.cs` | Update |
| `Persistence/Configurations/SubCategoryConfiguration.cs` | **New** |
| `Persistence/Configurations/SubCategoryGroupConfiguration.cs` | **New** |
| `Persistence/Configurations/BrandConfiguration.cs` | **New** |
| `Persistence/Configurations/ProductConfiguration.cs` | Update |
| `Persistence/Repositories/CategoryRepository.cs` | **New** |
| `Persistence/Repositories/SubCategoryRepository.cs` | **New** |
| `Persistence/Repositories/SubCategoryGroupRepository.cs` | **New** |
| `Persistence/Repositories/BrandRepository.cs` | **New** |
| `DependencyInjection.cs` | Update — register repositories |
| `Migrations/AddCategoryHierarchy.cs` | **New** (generated) |

### Web (`CapheVanPhong.Web`)
| File | Action |
|------|--------|
| `Components/Shared/Admin/AdminSidebar.razor` | Update |
| `Components/Pages/Admin/Categories/Index.razor` | **New** |
| `Components/Pages/Admin/Categories/Create.razor` | **New** |
| `Components/Pages/Admin/Categories/Edit.razor` | **New** |
| `Components/Pages/Admin/Categories/SubCategories/Index.razor` | **New** |
| `Components/Pages/Admin/Categories/SubCategories/Create.razor` | **New** |
| `Components/Pages/Admin/Categories/SubCategories/Edit.razor` | **New** |
| `Components/Pages/Admin/Categories/Groups/Index.razor` | **New** |
| `Components/Pages/Admin/Categories/Groups/Create.razor` | **New** |
| `Components/Pages/Admin/Categories/Groups/Edit.razor` | **New** |
| `Components/Pages/Admin/Brands/Index.razor` | **New** |
| `Components/Pages/Admin/Brands/Create.razor` | **New** |
| `Components/Pages/Admin/Brands/Edit.razor` | **New** |

---

## Product Filtering

The `Product` entity gains optional `SubCategoryId` and `BrandId` FKs, enabling queries like:

```csharp
// Filter by category slug
products.Where(p => p.Category.Slug == slug)

// Filter by sub-category slug
products.Where(p => p.SubCategory != null && p.SubCategory.Slug == slug)

// Filter by brand slug
products.Where(p => p.Brand != null && p.Brand.Slug == slug)
```

Public URL patterns:
- `/danh-muc/{categorySlug}` — filter by main category
- `/danh-muc/{categorySlug}/{subCategorySlug}` — filter by sub-category
- `/thuong-hieu/{brandSlug}` — filter by brand
