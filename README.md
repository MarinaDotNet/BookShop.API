# BookShop.API

## ASP.NET Core REST API — Portfolio Project

**BookShop.API** is a RESTful Web API built with **ASP.NET Core**. The goal of this project is to show how I work with backend development - clean architecture, real authentication flows, and practical patterns.

> **Project Status**
> 🚧 Still in active development - adding new features and improving existing ones.

## Live Demo (Swagger)

🔗[BookShop.API Live](https://bookshop-api-xyxs.onrender.com/swagger/index.html)

---

## What This Project Shows

I buiilt this to practive and demostrate skills that matter for a Junior .NET Backend Developer:

- Building REST APIs with clear structure and versioning
- Layered architecture with proper separation of concerns
- Full JWT authentication with refresh token rotation
- Email confirmation and account management flows
- Working with both MongoDB and PostgreSQL in the project
- Custom exception handling with RFC 7807 ProblemDetails
- Deploying to the cloud with Docker

---

## Tech Stack

- ASP .NET Core Web API (.NET 10)
- Entity Framework Core + PostgreSQL (users and auth)
- MongoDB (books catalog)
- JWT Bearer Authentication + Refresh Tokens
- ASP .NET Core Data Protection (auth action tokens)
- Brevo (transactional email)
- AutoMapper
- Swagger / OpenAPI (with XML documentation)
- Docker
- Render.com (deployment)

---

## API Versioning

Instead of versioning by feature, I versioned by **who can access what**:

| Version | Who can use it | Controllers |
|---------|----------------|-------------|
| **V1** | Admins only | `AuthConroller`, `BooksController` |
| **V2** | Logged in users | `BooksController` |
| **V3** | Guests / not logged in | `BooksController` |

This keeps authorization logic clear at the routing level and makes access boundaries explicit.

---

## Authentication & Security

The auth system is one of the main focuses of this project. It includes:

- **Registration** with email confirmation (resend with 3-minute cooldown)
- **Login** with JWT access token + refresh token pair
- **Refresh token rotation** - old token is revoked and replaced on every refresh
- **Reuse detection** - if a revoked token is used again, all sessions for that user are invalidated
- **Logout** (single session) and **logout from all devices**
- **SecurityTokenInvalidBeforeUtc** - when password changes or user logs out from all devices, all existing JWT tokens become invalid immediately, even before they expire
- **Password reset** via email link (with HTML form for browser flow and JSON endpoint for API clients)
- **Email change** with confirmation to the new address
- **Account deletion** with email confirmation
- **Account recovery** for soft-deleted accounts
- **Soft delete** - users are never removed from the database

---

## Project Structure

```
BookShop.API
|
├── Controllers
|  ├── V1
|  |  ├── AuthController.cs          ← Admin + public auth endpoints
|  |  └── BooksController.cs         ← Full CRUD (admin only)
|  ├── V2
|  |  └── BooksController.cs         ← Read all available books (logged in users)
|  ├── V3
|  |  └── BooksController.cs         ← Top 10 cheapest books (guests)
|  └── BaseApiController.cs          ← Shared base with GetCurrentUserId()
|
├── DTOs
|  ├── Auth
|  |  ├── UserRegisterDto.cs
|  |  ├── UserLogin Dto.cs
|  |  ├── UserDto.cs
|  |  ├── LoginResultDto.cs
|  |  ├── LogoutDto.cs
|  |  ├── AccountDeleteDto.cs
|  |  ├── AccountRequestDto.cs
|  |  ├── EmailDto.cs
|  |  ├── ForgotPasswordDto.cs
|  |  ├── ResendEmailConfirmationDto.cs
|  |  ├── ResetPasswordDto.cs
|  |  ├── UpdateEmailDto.cs
|  |  ├── UpdatePasswordDto.cs
|  |  └── UpdateUserNameDto.cs
|  └── Catalog
|  |  ├── BookDto.cs
|  |  ├── BookSearchRequestDto.cs
|  |  └── BookUpdateDto.cs
|
├── Exceptions
|  ├── ConflictException.cs
|  ├── ForbiddenException.cs
|  ├── InvalidTokenException.cs
|  ├── NotFoundExcekption.cs
|  └── ValidationException.cs
|
├── Infrastructure
|  ├──Persistence
|  |  ├── AuthDbContext.cs           ← EF Core context for PostgreSQL
|  |  ├── MongoDbContext.cs          ← MongoDB context for books
|  |  ├── MongoDbSettings.cs
|  |  └── UpdateDefinitionExtensions.cs
|  ├── AppUrlOptions.cs
|  ├── BrevoAuthEmailSender.cs
|  ├── BrevoOptions.cs
|  ├── ConfigurationSwaggerOptions.cs
|  └── JwtOptions.cs
|
├── Mappings
|  ├── BookMappingProfile.cs
|  └── UserMappingProfile.cs
|
├── Middleware
|  ├── ExceptionHandlingMiddleware.cs
|  └── ProblemDetailsBuilder.cs
|
├── Migrations
|
├── Models
|  ├── Auth
|  |  ├── AuthTokens.cs              ← Token purposes + payload model
|  |  ├── RefreshToken.cs
|  |  ├── Role.cs
|  |  ├── User.cs
|  |  └── UserRole.cs
|  ├── Catalog
|  |  └── Book.cs
|
├── Repositories
|  ├── BookRepository.cs
|  ├── IBookRepository.cs
|  ├── IUserRepository.cs
|  └── UserRepository.cs
|
├── Services
|  ├── AuthLinkGenerator.cs
|  ├── IAuthLinkGenerator.cs
|  ├── AuthServices.cs
|  ├── AuthTokenService.cs
|  ├── IAuthTokenService.cs
|  ├── BookService.cs
|  ├── IBookService.cs
|  ├── IAuthEmailSender.cs
|  ├── JwtTokenService.cs
|  ├── IJwtTokenService.cs
|  ├── RefreshTokenGenerator.cs
|  ├── IRefreshTokenGenerator.cs
|  ├── RefreshTokenHasher.cs
|  └── IRefreshTokenHasher.cs
|
├── Program.cs
├── appsettings.json
├── appsettings.Development.json
└── Dockerfile
```

---

## How It's Organized

**Controllers** handle HTTP only - no business logic, no direct data access.

**Services** contain all business logic. Every service has an interface. `AuthServices` is the main one - it handles everyting from registration to account recovery.

**Repositories** handle data access only. Books live in MongoDB, users and tokens live in PostgreSQL. Both sides have interface.

**DTOs** are the API contracts - separate from domain models, organized by Auth and Catalog.

**Middleware** catches all unhandled exceptions and converts them into consistent RFC 7807 `ProblemDetails` responses. The mapping from exception type to HTTP status code lives in `ProblemDetailsBuilder`.

**Infrastructure** holds database contexts, configuration options, email sending, and Swagger setup.

**Auth tokens** (email confirmation, password reset, etc.) use ASP .NET Core Data Protection - the are not JWT tokens. They are purpose-bound, time-limited, and Base64URL-encoded.

---

## Database Design Highlights

**PostgreSQL (vis EF Core):**
- Soft delete with `IsDeleted` flag and query filter so delted users are invisible by default
- Partial unique indexes on `NormalizedEmail` and `NormalizedUsername` filtered by `IsDeleted = false` - so a deleted user's email canbe reused after account recovery
- `SecurityTokenInvalidBeforeUtc` timestamp for instant JWT invalidation without a token blacklist

**MongoDB:**
- Books collection with case-insensitive regex search across title, authors, publisher, genres, and annotation
- Partial update support via `UpdateDefinition` builder

---

## What I'm Working On Next

**1. Bring Books up to the same level as Auth**

Starting with `BooksController`:
- Add and verify `ProducesResponseType` attributes on all endpints
- Improve XML docs to match the Swagger documentation style used in `AuthController`
- Review all routes: `GET /all`, `GET /{id}`, `GET /search-exact`, `GET /search-partial-match`, `POST /add`, `PUT /{id}`,  `PATCH /update-partly/{id}`, `DELETE /{id}`
- Decide which endpoints should be admin-only and which should be available to all logged-in users

**2. Polish `BookService`**

A few things that need attention:
- `GetAllBooksAsync` currently throws `NotFoundException` when no books are found - returning `200 []` is probably more correct for a list endpoint
- Imporve `BookDto` validation logic
- Review `UpdateBookAsync` and `UpdateBookPartlyAsync` for edge cases
- Make exception usage cosistent across the service (`NotFoundException`, `ValidationException`, `InvalidOperationException`)
- Fix XML docs that are outdated or don't match the current method signatures

**3. Clean up `BookRepository`**

- Fix XML doc type (`avialability`, `intendent`, etc.)
- Review the difference between exact match and partial match search behavior
- General code cleanup

**4. Extend `BooksController V2**

Add more endpoints for logged-in users - search by term, get by ID, and other read operations beyond just `GET \all`.

**5. Cart feature**
`CartController` and all supporting components - `ICartService`, `CartService`, `ICartRepository`, DTOs, and the Cart model. Cart data will likely live in Redis or MongoDB.

**6. Order feature**

`OrderController` and all supporting components - `IOrderService`, `OrderService`, `IOrderRepository`, `OrderRepository`, DTOs, and the Order model with PostgreSQL persistence.

