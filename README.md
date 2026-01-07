# BookShop.API
## ASP.NET Core REST API — Portfolio Project
**BookShop.API** is a production-oriented RESTful Web API built with **ASP.NET Core**, designed to demonstrate clean backend architecture, secure authentication, and maintainable code.
This project emphasises **engineering practices** and real-world API development rather than full feature completeness.

> Project Status\
> 🚧 Active Development:\
> continuously improving and extending backend functionality.

## Live Demo (Swagger)

🔗[BookShop.API Live](https://bookshop-api-xyxs.onrender.com/swagger/index.html)

## Why This Project Exists

This project demonstrates competencies relevant to a **Junior .NET Backend Developer** role:
* Designing RESTful APIs with clear contracts
* Applying layered architecture and separation of concerns
* Implementing secure authentication and role-based authorization
* Working with DTOs and AutoMapper for clean mapping
* Writing maintainable and scalable backend code
* Preparing APIs for real-world deployment and cloud hosting

## Technical Stack
* ASP.NET Core Web API
* Entity Framework Core
* MongoDB / Redis (NoSQL persistence)
* JWT Bearer Authentication
* AutoMapper
* Swagger / OpenAPI
* Render.com (Cloud deployment)

## Architectural Principles
The solution follows a layered **architecture**:
#### Controllers
Handle HTTP requests and responses only:
* BooksController.cs — Manages CRUD operations for books (GET, POST, PUT, DELETE)
### Services
Encapsulate business logic:
* BookService.cs — Implements business rules for book operations
### Repositories
Data access layer, abstracted via interfaces:
* IBookRepository.cs — Repository interface
* BookRepository.cs — Concrete implementation for MongoDB persistence
### Models & DTOs
Define domain entities and API contracts:
* Book.cs — Domain model for books
* BookDto.cs — API contract for books
* BookUpdateDto.cs / BookSearchRequestDto.cs — DTOs for update and search operations
### Mappings
Decouples domain models from DTOs:
* BookMapingProfile.cs — AutoMapper configuration for books
### Middleware
Custom pipeline components:
* ExceptionHandlingMiddleware.cs — Handles exceptions and produces standardized API responses
### Infrastructure
External dependencies and configuration:
* MongoDbContext.cs — MongoDB database context
* MongoDbSettings.cs — MongoDB configuration
* UpdateDefinitionExtensions.cs — Helper extensions for MongoDB update operations
### Exceptions
Custom exception types for precise error handling:
* ConflictException.cs
* ForbiddenException.cs
* NotFoundException.cs
* ValidationException.cs

## API Overview
### Books Management
* Create, read, update, delete operations
* Route and payload validation
* Admin-only write operations
### Security
* JWT-based authentication
* Role-based authorization
* Sensitive operations restricted to administrators
All endpoints are fully documented via **Swagger/OpenAPI**.

## Project Structure (Code Reflection)
```
BookShop.API
|
├── Controllers
|   └── BooksController.cs
|
├── Services
|   └── BookService.cs
|
├── Repositories
|   ├── IBookRepository.cs
|   └── BookRepository.cs
|
├── Models
|   ├── Book.cs
|   ├── BookDto.cs
|   ├── BookUpdateDto.cs
|   └── BookSearchRequestDto.cs
|
├── Mappings
|   └── BookMapingProfile.cs
|
├── Middleware
|   └── ExceptionHandlingMiddleware.cs
|
├── Infrastructure
|   └── Persistence
|       ├── MongoDbContext.cs
|       └── MongoDbSettings.cs
|   └── UpdateDefinitionExtensions.cs
|
├── Exceptions
|   ├── ConflictException.cs
|   ├── ForbiddenException.cs
|   ├── NotFoundException.cs
|   └── ValidationException.cs
|
├── Program.cs
└── appsettings.json
```

## What This Project Demonstrates
✔ Clean API design\
✔ Practical use of ASP.NET Core & MongoDB\
✔ JWT authentication and role-based authorisation\
✔ Layered architecture & dependency injection\
✔ Exception handling and maintainable codebase\

## Planned Improvements
* Centralised global exception handling enhancements
* Input validation improvements
* Pagination and filtering for book queries
* Unit and integration tests
* API versioning
* Logging and monitoring improvements
