# Simplify Catalog Management — Implementation Plan

## Overview

Simplify the catalog management system by removing the complex 3-level hierarchy (Category → SubCategoryGroup → SubCategory) and replacing it with a self-referencing Category model. This enables more flexible product categorization while reducing code complexity.

---

## Current vs. New Structure

### Current (Complex)
```
Category
├── SubCategoryGroup (1:N)
│   └── SubCategory (1:N)
│       └── Product (1:N) — via SubCategoryId
└── Product (1:N) — via CategoryId
```

**Problems:**
- Products can only be in ONE category path
- 3 separate entities to manage
- SubCategoryGroup often adds no real value
- Admin complexity with nested pages

### New (Simplified)
```
Category (self-referencing via ParentId)
├── Category (Level 1 — child)
│   └── Category (Level 2 — grandchild, MAX)
└── Product (M:N via ProductCategory)

Brand (independent)
└── Product (1:N)
```

**Benefits:**
- ✅ One product can belong to multiple categories (e.g., "Máy pha mini" + "Máy pha gia đình")
- ✅ Unlimited nesting with single entity (but validated to max 2 levels)
- ✅ Filtering by parent category automatically includes all children's products
- ✅ Simpler admin UI — one Category management page instead of three

---

## Entity Design

### `Category` (Updated)

| Field | Type | Notes |
|-------|------|-------|
| Id | int | PK |
| Name | string(200) | Required |
| Slug | string(200) | Unique, SEO-friendly |
| Description | string(1000) | |
| ImageName | string(255) | Nullable |
| DisplayOrder | int | For ordering |
| IsActive | bool | Show/hide |
| **ParentId** | int? | **NEW** — FK to self, nullable for root categories |
| **Level** | int | **NEW** — Auto-computed: 0=root, 1=child, 2=grandchild |
| CreatedAt, UpdatedAt | DateTime | From BaseEntity |

**Navigation Properties:**
- `Parent` — The parent category (null for root)
- `Children` — Collection of child categories
- `Products` — M:N via `ProductCategory` join table

**Business Rules:**
- `Level` is auto-computed based on `ParentId`:
  - If `ParentId` is null → Level = 0 (root)
  - If parent's Level = 0 → Level = 1 (child)
  - If parent's Level = 1 → Level = 2 (grandchild)
  - If parent's Level = 2 → **VALIDATION ERROR** (max depth exceeded)
- `Level` is NOT editable by admin — computed on create/update

### `ProductCategory` (NEW — Join Table)

| Field | Type | Notes |
|-------|------|-------|
| ProductId | int | FK → Product |
| CategoryId | int | FK → Category |

**Composite Primary Key:** (ProductId, CategoryId)

**Navigation Properties:**
- `Product` — The product
- `Category` — The category

### `Product` (Updated)

| Field | Type | Notes |
|-------|------|-------|
| ... | ... | Existing fields unchanged |
| ~~CategoryId~~ | ~~int~~ | **REMOVED** |
| ~~SubCategoryId~~ | ~~int?~~ | **REMOVED** |
| BrandId | int? | FK → Brand (KEPT as 1:N) |

**Navigation Properties:**
- `ProductCategories` — M:N collection (NEW)
- `Brand` — 1:N (KEPT)
- ~~`Category`~~ — **REMOVED**
- ~~`SubCategory`~~ — **REMOVED**

### `Brand` (Unchanged)

| Field | Type | Notes |
|-------|------|-------|
| Id | int | PK |
| Name | string(200) | Required |
| Slug | string(200) | Unique |
| Description | string(1000) | Nullable |
| LogoName | string(255) | Nullable |
| DisplayOrder | int | |
| IsActive | bool | |

**Navigation Properties:**
- `Products` — 1:N collection

---

## Domain Layer Changes

### Files to DELETE

| File | Reason |
|------|--------|
| `Entities/SubCategory.cs` | Entity removed |
| `Entities/SubCategoryGroup.cs` | Entity removed |
| `Interfaces/ISubCategoryRepository.cs` | No longer needed |
| `Interfaces/ISubCategoryGroupRepository.cs` | No longer needed |

### Files to CREATE

| File | Description |
|------|-------------|
| `Entities/ProductCategory.cs` | Join entity for M:N relationship |

### Files to UPDATE

#### `Entities/Category.cs`

```csharp
public class Category : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string? ImageName { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; } = true;
    
    // NEW: Self-referencing hierarchy
    public int? ParentId { get; private set; }
    public int Level { get; private set; } // Auto-computed, not settable
    
    public Category? Parent { get; private set; }
    public ICollection<Category> Children { get; private set; } = new List<Category>();
    
    // UPDATED: M:N via ProductCategory
    public ICollection<ProductCategory> ProductCategories { get; private set; } = new List<ProductCategory>();

    // Factory method updated to accept parentId and compute level
    public static Category Create(
        string name, 
        string slug, 
        string description, 
        int? parentId,
        int? parentLevel, // Provided by caller to compute Level
        string? imageName = null, 
        int displayOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tên danh mục không được để trống", nameof(name));
        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Slug không được để trống", nameof(slug));
        
        // Validate max depth (Level 2 = grandchild is max)
        int newLevel = parentId.HasValue ? (parentLevel ?? 0) + 1 : 0;
        if (newLevel > 2)
            throw new InvalidOperationException("Không thể tạo danh mục quá 2 cấp (gốc → con → cháu)");

        return new Category
        {
            Name = name,
            Slug = slug.ToLowerInvariant(),
            Description = description,
            ImageName = imageName,
            DisplayOrder = displayOrder,
            IsActive = true,
            ParentId = parentId,
            Level = newLevel,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        string name, 
        string slug, 
        string description, 
        int? parentId,
        int? parentLevel,
        string? imageName = null, 
        int displayOrder = 0)
    {
        // Similar validation as Create
        // Recompute Level if ParentId changed
    }
}
```

#### `Entities/Product.cs`

```csharp
public class Product : BaseEntity
{
    // ... existing fields ...
    
    // REMOVED: CategoryId, SubCategoryId, Category, SubCategory
    
    // NEW: M:N relationship
    public ICollection<ProductCategory> ProductCategories { get; private set; } = new List<ProductCategory>();
    
    // KEPT: Brand relationship (1:N)
    public int? BrandId { get; private set; }
    public Brand? Brand { get; private set; }
    
    // Updated factory method
    public static Product Create(
        string name,
        string slug,
        string? description,
        decimal? price,
        int brandId, // Required
        IEnumerable<int> categoryIds, // NEW: Multiple categories
        // ... other params
    )
    {
        // Validation...
        var product = new Product
        {
            Name = name,
            Slug = slug,
            BrandId = brandId,
            // ... other fields
        };
        
        // Add categories
        foreach (var categoryId in categoryIds)
            product.ProductCategories.Add(new ProductCategory { ProductId = 0, CategoryId = categoryId });
        
        return product;
    }
}
```

#### `Entities/ProductCategory.cs` (NEW)

```csharp
namespace CapheVanPhong.Domain.Entities;

public class ProductCategory
{
    public int ProductId { get; set; }
    public int CategoryId { get; set; }
    
    public Product Product { get; set; } = null!;
    public Category Category { get; set; } = null!;
}
```

#### `Interfaces/ICategoryRepository.cs`

```csharp
public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<IReadOnlyList<Category>> GetAllWithChildrenAsync(CancellationToken ct = default);
    Task<Category?> GetByIdWithChildrenAsync(int id, CancellationToken ct = default);
    Task<bool> SlugExistsAsync(string slug, int? excludeId = null, CancellationToken ct = default);
    
    // NEW: For hierarchy operations
    Task<IReadOnlyList<int>> GetAllDescendantIdsAsync(int parentCategoryId, CancellationToken ct = default);
    Task<IReadOnlyList<Category>> GetRootCategoriesAsync(CancellationToken ct = default);
}
```

#### `Interfaces/IProductRepository.cs`

```csharp
public interface IProductRepository : IRepository<Product>
{
    Task<IReadOnlyList<Product>> GetAllWithDetailsAsync(CancellationToken ct = default);
    Task<Product?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default);
    Task<bool> SlugExistsAsync(string slug, int? excludeId = null, CancellationToken ct = default);
    
    // NEW: Filter by multiple categories (for M:N)
    Task<IReadOnlyList<Product>> GetByCategoryIdsAsync(
        IEnumerable<int> categoryIds, 
        CancellationToken ct = default);
}
```

---

## Application Layer Changes

### Files to DELETE

| File | Reason |
|------|--------|
| `Services/ISubCategoryService.cs` | Entity removed |
| `Services/SubCategoryService.cs` | Entity removed |
| `Services/ISubCategoryGroupService.cs` | Entity removed |
| `Services/SubCategoryGroupService.cs` | Entity removed |

### Files to UPDATE

#### `Services/ICategoryService.cs`

```csharp
public interface ICategoryService
{
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Category>> GetAllWithChildrenAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Category>> GetRootCategoriesAsync(CancellationToken ct = default);
    Task<Category?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Category?> GetBySlugAsync(string slug, CancellationToken ct = default);
    
    // For filtering: get all descendant IDs for a parent category
    Task<IReadOnlyList<int>> GetAllDescendantIdsAsync(int parentCategoryId, CancellationToken ct = default);
    
    Task<(bool success, string? error)> CreateAsync(
        string name, string slug, string description, 
        int? parentId, string? imageName, int displayOrder, 
        CancellationToken ct = default);
    
    Task<(bool success, string? error)> UpdateAsync(
        int id, string name, string slug, string description, 
        int? parentId, string? imageName, int displayOrder, bool isActive,
        CancellationToken ct = default);
    
    Task<(bool success, string? error)> DeleteAsync(int id, CancellationToken ct = default);
}
```

#### `Services/CategoryService.cs`

- Update to handle `ParentId` and `Level` computation
- Validate max depth (Level ≤ 2) on Create/Update
- Prevent circular references (a category cannot be its own ancestor)

#### `Services/IProductService.cs`

```csharp
public interface IProductService
{
    Task<IReadOnlyList<Product>> GetAllWithDetailsAsync(CancellationToken ct = default);
    Task<Product?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default);
    
    // NEW: Filter by category (includes all descendants automatically)
    Task<IReadOnlyList<Product>> GetByCategoryAsync(int categoryId, CancellationToken ct = default);
    
    // NEW: Filter by brand
    Task<IReadOnlyList<Product>> GetByBrandAsync(int brandId, CancellationToken ct = default);
    
    Task<(bool success, string? error)> CreateAsync(
        string name, string slug,
        int brandId, // Required (1:N)
        IReadOnlyList<int> categoryIds, // NEW: Multiple categories (M:N)
        bool isAvailable,
        decimal? price,
        // ... other params
        CancellationToken ct = default);
    
    Task<(bool success, string? error)> UpdateAsync(
        int id, string name, string slug,
        int brandId,
        IReadOnlyList<int> categoryIds, // NEW: Multiple categories
        // ... other params
        CancellationToken ct = default);
    
    Task<(bool success, string? error)> DeleteAsync(int id, CancellationToken ct = default);
}
```

#### `Services/ProductService.cs`

- Replace `categoryId`/`subCategoryId` params with `categoryIds` list
- Update `ValidateReferencesAsync` to validate multiple categories
- Update Create/Update to manage `ProductCategories` collection

---

## Infrastructure Layer Changes

### Files to DELETE

| File | Reason |
|------|--------|
| `Persistence/Configurations/SubCategoryConfiguration.cs` | Entity removed |
| `Persistence/Configurations/SubCategoryGroupConfiguration.cs` | Entity removed |
| `Persistence/Repositories/SubCategoryRepository.cs` | Entity removed |
| `Persistence/Repositories/SubCategoryGroupRepository.cs` | Entity removed |

### Files to CREATE

| File | Description |
|------|-------------|
| `Persistence/Configurations/ProductCategoryConfiguration.cs` | Join table configuration |
| `Persistence/Repositories/ProductCategoryRepository.cs` | Optional, for direct queries |

### Files to UPDATE

#### `Persistence/AppDbContext.cs`

```csharp
public DbSet<Category> Categories => Set<Category>();
public DbSet<Brand> Brands => Set<Brand>();
public DbSet<Product> Products => Set<Product>();
public DbSet<ProductImage> ProductImages => Set<ProductImage>();
public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>(); // NEW
public DbSet<Order> Orders => Set<Order>();
public DbSet<OrderItem> OrderItems => Set<OrderItem>();

// REMOVED: SubCategories, SubCategoryGroups
```

#### `Persistence/Configurations/CategoryConfiguration.cs`

```csharp
public void Configure(EntityTypeBuilder<Category> builder)
{
    builder.HasKey(c => c.Id);

    builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
    builder.Property(c => c.Slug).IsRequired().HasMaxLength(200);
    builder.HasIndex(c => c.Slug).IsUnique();
    builder.Property(c => c.Description).HasMaxLength(1000);
    builder.Property(c => c.ImageName).HasMaxLength(255);
    builder.Property(c => c.DisplayOrder).HasDefaultValue(0);
    builder.Property(c => c.IsActive).HasDefaultValue(true);
    builder.Property(c => c.Level).IsRequired();

    // Self-referencing relationship
    builder.HasOne(c => c.Parent)
        .WithMany(c => c.Children)
        .HasForeignKey(c => c.ParentId)
        .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

    // M:N with Product via ProductCategory
    builder.HasMany(c => c.ProductCategories)
        .WithOne(pc => pc.Category)
        .HasForeignKey(pc => pc.CategoryId)
        .OnDelete(DeleteBehavior.Cascade);
}
```

#### `Persistence/Configurations/ProductConfiguration.cs`

```csharp
public void Configure(EntityTypeBuilder<Product> builder)
{
    // ... existing property configs ...

    // REMOVED: Category relationship (was 1:N)
    // REMOVED: SubCategory relationship

    // KEPT: Brand relationship (1:N)
    builder.HasOne(p => p.Brand)
        .WithMany(b => b.Products)
        .HasForeignKey(p => p.BrandId)
        .OnDelete(DeleteBehavior.Restrict);

    // NEW: M:N with Category via ProductCategory
    builder.HasMany(p => p.ProductCategories)
        .WithOne(pc => pc.Product)
        .HasForeignKey(pc => pc.ProductId)
        .OnDelete(DeleteBehavior.Cascade);

    // ... images, etc ...
}
```

#### `Persistence/Configurations/ProductCategoryConfiguration.cs` (NEW)

```csharp
public class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
{
    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        builder.HasKey(pc => new { pc.ProductId, pc.CategoryId });

        builder.HasOne(pc => pc.Product)
            .WithMany(p => p.ProductCategories)
            .HasForeignKey(pc => pc.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pc => pc.Category)
            .WithMany(c => c.ProductCategories)
            .HasForeignKey(pc => pc.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

#### `Persistence/Repositories/CategoryRepository.cs`

- Add `GetAllDescendantIdsAsync` implementation using recursive CTE or in-memory traversal
- Add `GetRootCategoriesAsync` implementation

### Migration

```bash
dotnet ef migrations add SimplifyCatalog \
  --project src/Infrastructure/CapheVanPhong.Infrastructure \
  --startup-project src/Presentation/CapheVanPhong.Web
```

**Migration will:**
1. Create `ProductCategories` table (ProductId, CategoryId)
2. Add `ParentId` and `Level` columns to `Categories`
3. Migrate existing data:
   - For each Product, create ProductCategory from existing CategoryId
   - For each Product with SubCategoryId, create additional ProductCategory
   - Set ParentId null and Level 0 for existing categories
4. Drop `SubCategoryId` column from `Products`
5. Drop `CategoryId` column from `Products` (after migrating data)
6. Drop `SubCategories` table
7. Drop `SubCategoryGroups` table

---

## Presentation Layer Changes

### Files to DELETE

| File | Reason |
|------|--------|
| `Components/Pages/Admin/Categories/SubCategories/*` | SubCategory removed |
| `Components/Pages/Admin/Categories/Groups/*` | SubCategoryGroup removed |

### Files to UPDATE

#### `Components/Pages/Admin/Categories/Index.razor`

- Show hierarchy tree view (indented children)
- Display Level badge (Root/Child/Grandchild)
- Add "Add Child Category" action for categories with Level < 2

#### `Components/Pages/Admin/Categories/Create.razor`

- Add "Parent Category" dropdown (shows only categories with Level < 2)
- Remove SubCategory/Group related fields
- Show computed Level preview

#### `Components/Pages/Admin/Categories/Edit.razor`

- Add "Parent Category" dropdown
- Show current Level (read-only)
- Validate on parent change (prevent exceeding max depth)

#### `Components/Pages/Admin/Products/Create.razor`

- Replace single category dropdown with **multi-select** for categories
- Keep single brand dropdown

#### `Components/Pages/Admin/Products/Edit.razor`

- Replace single category dropdown with **multi-select** for categories
- Keep single brand dropdown

#### `Components/Shared/Admin/AdminSidebar.razor`

- Remove SubCategories and Groups menu items
- Simplify Categories menu to single entry

### Public Filtering Logic

When a user clicks a category in the public menu:

```csharp
// In a public service or component
public async Task<IReadOnlyList<Product>> GetProductsByCategoryAsync(int categoryId)
{
    // Get the category and all its descendants
    var categoryIds = await _categoryService.GetAllDescendantIdsAsync(categoryId);
    
    // Query products in any of those categories (deduplicated by DISTINCT)
    return await _productRepository.GetByCategoryIdsAsync(categoryIds);
}
```

**URL Patterns:**
- `/danh-muc/{categorySlug}` — Filter by category (includes all children)
- `/thuong-hieu/{brandSlug}` — Filter by brand

**Combined Filtering:**
- `/danh-muc/{categorySlug}?brand={brandSlug}` — Category + Brand filter

---

## Summary of Files Changed

### Domain Layer

| File | Action |
|------|--------|
| `Entities/Category.cs` | **UPDATE** — Add ParentId, Level, Children; change Products to ProductCategories |
| `Entities/Product.cs` | **UPDATE** — Remove CategoryId/SubCategoryId; add ProductCategories |
| `Entities/ProductCategory.cs` | **NEW** — Join entity |
| `Entities/SubCategory.cs` | **DELETE** |
| `Entities/SubCategoryGroup.cs` | **DELETE** |
| `Interfaces/ICategoryRepository.cs` | **UPDATE** — Add hierarchy methods |
| `Interfaces/IProductRepository.cs` | **UPDATE** — Add M:N filtering |
| `Interfaces/ISubCategoryRepository.cs` | **DELETE** |
| `Interfaces/ISubCategoryGroupRepository.cs` | **DELETE** |

### Application Layer

| File | Action |
|------|--------|
| `Services/ICategoryService.cs` | **UPDATE** — Add ParentId, Level handling |
| `Services/CategoryService.cs` | **UPDATE** — Implement hierarchy logic |
| `Services/IProductService.cs` | **UPDATE** — Replace single category with multiple |
| `Services/ProductService.cs` | **UPDATE** — Implement M:N product-category |
| `Services/ISubCategoryService.cs` | **DELETE** |
| `Services/SubCategoryService.cs` | **DELETE** |
| `Services/ISubCategoryGroupService.cs` | **DELETE** |
| `Services/SubCategoryGroupService.cs` | **DELETE** |
| `DependencyInjection.cs` | **UPDATE** — Remove deleted services |

### Infrastructure Layer

| File | Action |
|------|--------|
| `Persistence/AppDbContext.cs` | **UPDATE** — Add ProductCategories DbSet; remove SubCategories, SubCategoryGroups |
| `Persistence/Configurations/CategoryConfiguration.cs` | **UPDATE** — Add self-reference and M:N |
| `Persistence/Configurations/ProductConfiguration.cs` | **UPDATE** — Remove Category FK; add M:N |
| `Persistence/Configurations/ProductCategoryConfiguration.cs` | **NEW** |
| `Persistence/Configurations/SubCategoryConfiguration.cs` | **DELETE** |
| `Persistence/Configurations/SubCategoryGroupConfiguration.cs` | **DELETE** |
| `Persistence/Repositories/CategoryRepository.cs` | **UPDATE** — Add hierarchy methods |
| `Persistence/Repositories/ProductRepository.cs` | **UPDATE** — Add M:N filtering |
| `Persistence/Repositories/SubCategoryRepository.cs` | **DELETE** |
| `Persistence/Repositories/SubCategoryGroupRepository.cs` | **DELETE** |
| `DependencyInjection.cs` | **UPDATE** — Remove deleted repos |
| `Migrations/...` | **NEW** — Generated migration |

### Presentation Layer

| File | Action |
|------|--------|
| `Components/Pages/Admin/Categories/Index.razor` | **UPDATE** — Tree view |
| `Components/Pages/Admin/Categories/Create.razor` | **UPDATE** — Parent picker |
| `Components/Pages/Admin/Categories/Edit.razor` | **UPDATE** — Parent picker, Level display |
| `Components/Pages/Admin/Categories/SubCategories/*` | **DELETE** |
| `Components/Pages/Admin/Categories/Groups/*` | **DELETE** |
| `Components/Pages/Admin/Products/Create.razor` | **UPDATE** — Multi-select categories |
| `Components/Pages/Admin/Products/Edit.razor` | **UPDATE** — Multi-select categories |
| `Components/Shared/Admin/AdminSidebar.razor` | **UPDATE** — Simplify menu |

---

## Validation Rules Summary

| Rule | Entity | Description |
|------|--------|-------------|
| Max depth | Category | Level ≤ 2 (root → child → grandchild) |
| Auto Level | Category | Level computed from ParentId, not editable |
| No circular ref | Category | A category cannot be its own ancestor |
| At least one category | Product | Product must have at least one ProductCategory |
| Unique combination | ProductCategory | Same product cannot have same category twice |

---

## Execution Order

1. **Domain Layer**
   - Create `ProductCategory.cs`
   - Update `Category.cs` with ParentId, Level
   - Update `Product.cs` with ProductCategories
   - Delete `SubCategory.cs`, `SubCategoryGroup.cs`
   - Update repository interfaces

2. **Infrastructure Layer**
   - Create `ProductCategoryConfiguration.cs`
   - Update `CategoryConfiguration.cs`, `ProductConfiguration.cs`
   - Delete SubCategory/SubCategoryGroup configurations
   - Update `AppDbContext.cs`
   - Update repositories
   - Delete SubCategory/SubCategoryGroup repositories
   - Generate migration

3. **Application Layer**
   - Update `ICategoryService`, `CategoryService`
   - Update `IProductService`, `ProductService`
   - Delete SubCategory/SubCategoryGroup services
   - Update `DependencyInjection.cs`

4. **Presentation Layer**
   - Update Category pages
   - Update Product pages
   - Update AdminSidebar
   - Delete SubCategory/Group pages

5. **Testing**
   - Run migration
   - Verify data migration
   - Test admin CRUD operations
   - Test public filtering