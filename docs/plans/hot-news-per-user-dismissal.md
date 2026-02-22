# Hot News — Per-User Dismissal (Server-Side) Plan

## Overview

Replace the current `localStorage`-based dismissal with a server-side approach so that
"do not remind for 3 days" is tied to the **user account**, not the browser. Anonymous
visitors continue to use `localStorage` as a fallback.

---

## Current Behavior

- Dismissal is stored in `localStorage` keyed by `hot_news_dismissed_{id}`.
- Works per browser/device only — same user on another device will still see the popup.
- Anonymous and authenticated users are treated identically.

---

## Target Behavior

| User type | Dismissal stored in | Scope |
|---|---|---|
| Anonymous | `localStorage` (unchanged) | Per browser |
| Authenticated (`User`, `Admin`, `SuperAdmin`) | DB table | Per user account, all devices |

---

## Implementation Plan

### 1. Domain — New Entity

**File:** `src/Core/CapheVanPhong.Domain/Entities/HotNewsDismissal.cs`

```csharp
public class HotNewsDismissal : BaseEntity
{
    public string UserId { get; private set; } = string.Empty;
    public int HotNewsId { get; private set; }
    public DateTime DismissedAt { get; private set; }

    private HotNewsDismissal() { }

    public static HotNewsDismissal Create(string userId, int hotNewsId) => new()
    {
        UserId = userId,
        HotNewsId = hotNewsId,
        DismissedAt = DateTime.UtcNow
    };
}
```

### 2. Domain — Repository Interface

**File:** `src/Core/CapheVanPhong.Domain/Interfaces/IHotNewsDismissalRepository.cs`

```csharp
public interface IHotNewsDismissalRepository : IRepository<HotNewsDismissal>
{
    Task<IReadOnlyList<int>> GetDismissedNewsIdsAsync(
        string userId,
        TimeSpan window,
        CancellationToken cancellationToken = default);

    Task UpsertAsync(
        string userId,
        int hotNewsId,
        CancellationToken cancellationToken = default);
}
```

`GetDismissedNewsIdsAsync` returns the list of `HotNewsId`s dismissed within the given
`window` (e.g. 3 days) for a user — used to filter before sending news to the client.

`UpsertAsync` inserts a new row or updates `DismissedAt` if one already exists for
`(UserId, HotNewsId)`.

### 3. Application — Service Interface Extension

**File:** `src/Core/CapheVanPhong.Application/Services/IHotNewsService.cs`

Add two methods:

```csharp
Task<IReadOnlyList<HotNews>> GetActiveForUserAsync(
    string userId,
    CancellationToken cancellationToken = default);

Task<(bool success, string? error)> DismissAsync(
    string userId,
    int hotNewsId,
    CancellationToken cancellationToken = default);
```

`GetActiveForUserAsync` returns active news excluding those dismissed within 3 days by
the given user — replaces `GetActiveAsync` for authenticated requests.

### 4. Application — Service Implementation

**File:** `src/Core/CapheVanPhong.Application/Services/HotNewsService.cs`

```csharp
public async Task<IReadOnlyList<HotNews>> GetActiveForUserAsync(
    string userId,
    CancellationToken cancellationToken = default)
{
    var dismissed = await _dismissalRepo.GetDismissedNewsIdsAsync(
        userId, TimeSpan.FromDays(3), cancellationToken);
    var all = await _hotNewsRepo.GetActiveAsync(cancellationToken);
    return all.Where(n => !dismissed.Contains(n.Id)).ToList();
}

public async Task<(bool success, string? error)> DismissAsync(
    string userId, int hotNewsId, CancellationToken cancellationToken = default)
{
    try
    {
        await _dismissalRepo.UpsertAsync(userId, hotNewsId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return (true, null);
    }
    catch (Exception ex)
    {
        return (false, ex.Message);
    }
}
```

### 5. Infrastructure — EF Configuration

**File:** `src/Infrastructure/CapheVanPhong.Infrastructure/Persistence/Configurations/HotNewsDismissalConfiguration.cs`

```csharp
builder.ToTable("HotNewsDismissals");
builder.HasKey(x => x.Id);
builder.Property(x => x.UserId).IsRequired().HasMaxLength(450);
builder.Property(x => x.HotNewsId).IsRequired();
builder.HasIndex(x => new { x.UserId, x.HotNewsId }).IsUnique();
```

### 6. Infrastructure — Repository

**File:** `src/Infrastructure/CapheVanPhong.Infrastructure/Persistence/Repositories/HotNewsDismissalRepository.cs`

```csharp
public async Task<IReadOnlyList<int>> GetDismissedNewsIdsAsync(
    string userId, TimeSpan window, CancellationToken cancellationToken = default)
{
    var since = DateTime.UtcNow - window;
    return await _dbSet
        .Where(d => d.UserId == userId && d.DismissedAt >= since)
        .Select(d => d.HotNewsId)
        .ToListAsync(cancellationToken);
}

public async Task UpsertAsync(
    string userId, int hotNewsId, CancellationToken cancellationToken = default)
{
    var existing = await _dbSet
        .FirstOrDefaultAsync(d => d.UserId == userId && d.HotNewsId == hotNewsId,
            cancellationToken);
    if (existing is null)
        _dbSet.Add(HotNewsDismissal.Create(userId, hotNewsId));
    else
        // Update DismissedAt — requires making the setter internal or adding an Update method
        existing.ResetDismissedAt();
}
```

Add `ResetDismissedAt()` to the domain entity:
```csharp
public void ResetDismissedAt() => DismissedAt = DateTime.UtcNow;
```

### 7. Infrastructure — DI Registration

**File:** `src/Infrastructure/CapheVanPhong.Infrastructure/DependencyInjection.cs`

```csharp
services.AddScoped<IHotNewsDismissalRepository, HotNewsDismissalRepository>();
```

**File:** `src/Core/CapheVanPhong.Application/DependencyInjection.cs`

No change needed — `HotNewsService` already registered; inject `IHotNewsDismissalRepository`
via constructor.

### 8. Presentation — Dismiss API Endpoint

Add a minimal API endpoint so the browser can POST a dismissal without a full page reload.

**File:** `src/Presentation/CapheVanPhong.Web/Endpoints/HotNewsEndpoints.cs` (new file)

```csharp
app.MapPost("/api/hot-news/{id:int}/dismiss", async (
    int id,
    IHotNewsService svc,
    ClaimsPrincipal user,
    CancellationToken ct) =>
{
    if (!user.Identity?.IsAuthenticated ?? true)
        return Results.Unauthorized();
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
    var (success, _) = await svc.DismissAsync(userId, id, ct);
    return success ? Results.Ok() : Results.StatusCode(500);
}).RequireAuthorization();
```

Register in `Program.cs`:
```csharp
app.MapHotNewsEndpoints(); // extension method on WebApplication
```

### 9. Home.razor.cs — Use Per-User Query When Authenticated

**File:** `src/Presentation/CapheVanPhong.Web/Components/Pages/Public/Home.razor.cs`

Inject `IHttpContextAccessor` (or use `AuthenticationStateProvider`) to check if the
current user is authenticated and get their ID:

```csharp
[Inject] private IHttpContextAccessor HttpContextAccessor { get; set; } = default!;

private async Task LoadActiveHotNewsAsync()
{
    var userId = HttpContextAccessor.HttpContext?
        .User?.FindFirstValue(ClaimTypes.NameIdentifier);

    IReadOnlyList<HotNews> news;
    if (!string.IsNullOrEmpty(userId))
        news = await HotNewsService.GetActiveForUserAsync(userId);
    else
        news = await HotNewsService.GetActiveAsync();

    ActiveHotNews = news.Select(...).ToList();
}
```

### 10. Layout.razor — Call Dismiss API for Authenticated Users

Update the `hotNewsDismiss` onclick handler to detect auth state and POST to the API
instead of (or in addition to) writing to `localStorage`:

```javascript
document.getElementById('hotNewsDismiss').onclick = function () {
    // Always update localStorage as fallback
    localStorage.setItem('hot_news_dismissed_' + news.id, now.toString());

    // If user is authenticated, also persist server-side
    var isAuth = document.body.dataset.userAuthenticated === 'true';
    if (isAuth) {
        fetch('/api/hot-news/' + news.id + '/dismiss', {
            method: 'POST',
            headers: { 'RequestVerificationToken': getAntiforgeryToken() }
        });
    }

    $('#hotNewsModal').modal('hide');
};
```

Pass auth state from server to the layout via a `data-user-authenticated` attribute on
`<body>` (set in `App.razor` or `Layout.razor`).

Add a helper to get the antiforgery token from the hidden `<input>` or meta tag that
Blazor already emits.

---

## Migration

After implementation, generate a new EF Core migration:

```
dotnet ef migrations add AddHotNewsDismissals \
  --project src/Infrastructure/CapheVanPhong.Infrastructure \
  --startup-project src/Presentation/CapheVanPhong.Web
dotnet ef database update \
  --project src/Infrastructure/CapheVanPhong.Infrastructure \
  --startup-project src/Presentation/CapheVanPhong.Web
```

---

## Files to Create

| File | Action |
|---|---|
| `Domain/Entities/HotNewsDismissal.cs` | Create |
| `Domain/Interfaces/IHotNewsDismissalRepository.cs` | Create |
| `Infrastructure/Configurations/HotNewsDismissalConfiguration.cs` | Create |
| `Infrastructure/Repositories/HotNewsDismissalRepository.cs` | Create |
| `Web/Endpoints/HotNewsEndpoints.cs` | Create |

## Files to Modify

| File | Change |
|---|---|
| `Application/Services/IHotNewsService.cs` | Add `GetActiveForUserAsync` + `DismissAsync` |
| `Application/Services/HotNewsService.cs` | Implement new methods |
| `Application/DependencyInjection.cs` | Inject `IHotNewsDismissalRepository` into service |
| `Infrastructure/DependencyInjection.cs` | Register `IHotNewsDismissalRepository` |
| `Infrastructure/Persistence/AppDbContext.cs` | Add `DbSet<HotNewsDismissal>` |
| `Web/Components/Pages/Public/Home.razor.cs` | Use `GetActiveForUserAsync` for auth users |
| `Web/Components/Layout/Layout.razor` | POST dismiss to API + `localStorage` fallback |
| `Web/Program.cs` | Register dismiss endpoint |
