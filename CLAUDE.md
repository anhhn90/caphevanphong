# Project Instructions for Claude Code

## Project Overview
**Cà Phê Văn Phòng** — a Vietnamese café website built with Blazor SSR (.NET 8+), Clean Architecture, EF Core, and MSSQL.

Two distinct areas:
- **Public Area** — accessible to all visitors; authenticated `User` role holders can view/manage their own orders
- **Admin Area** — restricted to `Admin` and `SuperAdmin` roles; full management of the café

For technical architecture, coding conventions, and template integration details, see [docs/technical.md](docs/technical.md).

---

## Technology Stack
- **Framework:** Blazor Server-Side Rendering (.NET 8+)
- **Architecture:** Clean Architecture (Domain → Application → Infrastructure → Presentation)
- **ORM:** Entity Framework Core + MSSQL
- **Auth:** ASP.NET Core Identity with RBAC
- **UI:** Bootstrap 3 (Admin area), Bootstrap 4 (Public area) — mobile-first

---

## Roles & Authorization

| Role | Description |
|---|---|
| `SuperAdmin` | Highest privilege. Can do everything `Admin` can, **plus** create/edit/delete `SuperAdmin` accounts. Seeded at runtime via `DatabaseSeeder`. |
| `Admin` | Full Admin area access. Can manage menu, orders, and non-SuperAdmin users. |
| `User` | Authenticated customer. Can view and edit their own orders in the Public area. |
| *(anonymous)* | Can browse the public menu. |

### SuperAdmin Restrictions
Only `SuperAdmin` can:
- Create a user with the `SuperAdmin` role
- Edit or reset the password of a `SuperAdmin` user
- Delete a `SuperAdmin` user

These restrictions are enforced both in the **UI** (buttons/dropdowns hidden) and **server-side** (handler guards return early with an error).

### Route Protection
- Admin pages: `@attribute [Authorize(Roles = "Admin,SuperAdmin")]`
- User-specific pages (e.g. order history): `@attribute [Authorize(Roles = "User,Admin,SuperAdmin")]`
- Conditional UI: `<AuthorizeView Roles="Admin,SuperAdmin">...</AuthorizeView>`
- SuperAdmin check in code: `HttpContext?.User.IsInRole("SuperAdmin")`
- Redirect unauthorized users to `/account/login` (configured in `Program.cs`)

### Identity Setup
- ASP.NET Core Identity integrated with EF Core (`IdentityDbContext`)
- Seed default roles (`SuperAdmin`, `Admin`, `User`) in the `InitialCreate` migration via `InsertData`
- Seed the default SuperAdmin account at runtime via `DatabaseSeeder` (password hashing requires runtime execution)
- Identity tables stored in the same MSSQL database

---

## Language & UI
- **Public area**: all text, labels, and messages in **Vietnamese (Tiếng Việt)**
- **Admin area**: all text, labels, and messages in **English**
- Use a Vietnamese-compatible Google Font (e.g. **Be Vietnam Pro**) in `wwwroot/css/app.css`
- All pages must be fully **mobile-first responsive**

---

## Important Rules
1. **Never** put business logic in Blazor components — delegate to Application layer services
2. **Never** reference the Infrastructure layer directly from components (except DI registration in `Program.cs`)
3. **Always** use the Repository pattern — no `DbContext` outside of Infrastructure
4. **Always** use `CancellationToken` in async methods
5. **Always** validate inputs at the Application layer using FluentValidation
6. Keep the **Domain layer** free of any framework dependencies
7. **Always** protect Admin pages with `[Authorize(Roles = "Admin,SuperAdmin")]` — never rely on UI-only hiding
8. **Always** scope user data queries to the logged-in user's ID (never expose other users' data)
9. **Always** use a Vietnamese-compatible font; write all UI text in **Vietnamese** (Public) and **English** (Admin)
10. **Always** design components mobile-first — start with the smallest screen size and scale up
