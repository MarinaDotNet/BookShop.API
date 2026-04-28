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
