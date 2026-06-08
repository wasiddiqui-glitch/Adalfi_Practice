# Library Management System — Architecture & Workflow

---

## 1. Clean Architecture — Layer Overview

```mermaid
graph TD
    subgraph API["🌐 API Layer — LibraryManagement.API"]
        Controllers["Controllers\nAuthController · BooksController\nAdminController · ReservationsController"]
        Middleware["Middleware\nTokenRevocationMiddleware"]
        Handlers["Delegating Handlers\nLoggingDelegatingHandler · RetryDelegatingHandler"]
    end

    subgraph Infrastructure["🗄️ Infrastructure Layer — LibraryManagement.Infrastructure"]
        Repos["Repositories\nBookRepository · UserBookRepository · ReservationRepository"]
        UoW["Unit of Work\nUnitOfWork"]
        DbCtx["AppDbContext\nEF Core + Identity"]
    end

    subgraph Application["⚙️ Application Layer — LibraryManagement.Application"]
        Services["Services\nAuthService · BookService\nAdminService · ReservationService"]
        Interfaces["Interfaces\nIBookRepository · IUnitOfWork\nIBookService · IAuthService"]
        DTOs["DTOs\nBookDto · AuthDtos · ReservationDtos"]
    end

    subgraph Domain["🧱 Domain Layer — LibraryManagement.Domain"]
        Models["Models\nBook · BookCopy · UserBook\nReservation · ApplicationUser · Fine"]
    end

    API --> Application
    API --> Infrastructure
    Infrastructure --> Application
    Infrastructure --> Domain
    Application --> Domain

    style Domain fill:#2d6a4f,color:#fff
    style Application fill:#1d3557,color:#fff
    style Infrastructure fill:#457b9d,color:#fff
    style API fill:#e63946,color:#fff
```

> **Key rule:** Dependencies point inward only. `Application` never knows EF exists. `Domain` knows nothing outside itself.

---

## 2. HTTP Request Pipeline

```mermaid
flowchart LR
    Client(["Client\nReact / Postman"])

    Client -->|HTTP Request + JWT| CORS

    subgraph Pipeline["ASP.NET Middleware Pipeline"]
        CORS["UseCors\nAllow localhost:5173"]
        Auth["UseAuthentication\nRead JWT → populate User"]
        TRM["TokenRevocationMiddleware\nIs token in DB?\nNO → 401 stop\nYES → continue"]
        Authz["UseAuthorization\nCheck Authorize attribute\nCheck Admin role"]
    end

    CORS --> Auth --> TRM --> Authz --> Controller(["Controller"])
    Controller -->|JSON Response| Client

    style TRM fill:#e63946,color:#fff
    style Auth fill:#457b9d,color:#fff
    style Authz fill:#1d3557,color:#fff
```

---

## 3. Full Application Workflow

```mermaid
flowchart TD
    Client(["Client"])

    Client --> AC & BC & ADC & RC

    subgraph Controllers["Controllers — API Layer"]
        AC["AuthController\nPOST /auth/register\nPOST /auth/login\nPOST /auth/logout\nPOST /auth/refresh"]
        BC["BooksController\nGET /api/books\nGET /api/books/my-books\nGET /api/books/history\nPOST /api/books/:id/checkout\nPOST /api/books/:id/return\nPOST /api/books/:id/report-faulty"]
        ADC["AdminController\nGET /api/admin/users\nGET /api/admin/checkouts\nGET /api/admin/overdue\nGET /api/admin/books/:id\nPOST /api/admin/books\nPUT /api/admin/books/:id\nDELETE /api/admin/books/:id\nPOST /api/admin/books/:id/copies\nDELETE /api/admin/copies/:id"]
        RC["ReservationsController\nGET /api/reservations\nPOST /api/reservations/:bookId\nDELETE /api/reservations/:id"]
    end

    AC --> AS
    BC --> BKS
    ADC --> ADS
    RC --> RS

    subgraph Services["Services — Application Layer"]
        AS["AuthService\n─────────────\nRegisterAsync\n  → create user via UserManager\n  → generate JWT + refresh token\n  → store tokens in DB\nLoginAsync\n  → verify password\n  → generate JWT + refresh token\nLogoutAsync\n  → remove tokens from DB\nRefreshAsync\n  → validate refresh token\n  → issue new token pair"]
        BKS["BookService\n─────────────\nGetAvailableBooksAsync\n  → all books with copy counts\nCheckoutBookAsync\n  → enforce max 5 book limit\n  → check reservation queue\n  → find available copy\n  → mark copy checked out\n  → create UserBook record\n  → fulfill reservation if any\n  → SaveChanges\nReturnBookAsync\n  → mark copy available\n  → set ReturnedAt\n  → SaveChanges\nMarkFaultyAsync\n  → mark copy faulty\n  → auto-return the book\n  → SaveChanges"]
        ADS["AdminService\n─────────────\nAddBookAsync\n  → create Book + N copies\nUpdateBookAsync\n  → update title/author/genre\nDeleteBookAsync\n  → block if copies checked out\nAddCopyAsync / DeleteCopyAsync\nGetAllActiveCheckoutsAsync\nGetOverdueCheckoutsAsync"]
        RS["ReservationService\n─────────────\nReserveAsync\n  → check book exists\n  → check not already borrowed\n  → check not already in queue\n  → add Reservation\nCancelAsync\n  → soft-delete: set CancelledAt\nGetUserReservationsAsync\n  → return queue position"]
    end

    AS --> UM
    BKS & ADS & RS --> UoW

    subgraph IdentityBox["ASP.NET Identity"]
        UM["UserManager\nCreateAsync\nCheckPasswordAsync\nSetAuthenticationTokenAsync\nRemoveAuthenticationTokenAsync"]
    end

    subgraph UoWBox["Unit of Work — Infrastructure Layer"]
        UoW["UnitOfWork\n─────────────\nShares ONE AppDbContext\nacross all repositories\n─────────────\nSaveChangesAsync\n→ commits all changes\n   in one transaction"]
    end

    UoW --> BR & UBR & RR

    subgraph RepoBox["Repositories — Infrastructure Layer"]
        BR["BookRepository\nGetAllWithCopiesAsync\nGetByIdWithCopiesAsync\nGetAvailableCopyAsync\nGetCopyByIdAsync\nExistsAsync\nAdd / Remove\nAddCopy / RemoveCopy"]
        UBR["UserBookRepository\nGetActiveByUser\nGetActiveByUserAndBook\nCountActiveByUser\nGetHistoryByUser\nGetAllActive\nGetAllOverdue"]
        RR["ReservationRepository\nGetActiveByUser\nGetActiveByBook\nGetActiveByUserAndBook\nCountActiveByBook\nGetByIdAsync\nAdd"]
    end

    BR & UBR & RR & UM --> DBCtx

    subgraph DBBox["Data Layer — Infrastructure"]
        DBCtx["AppDbContext\nIdentityDbContext\n─────────────\nDbSet Books\nDbSet BookCopies\nDbSet UserBooks\nDbSet Reservations\nDbSet Fines\nDbSet AuditLogs\n+ Identity Tables"]
    end

    DBCtx --> DB[("MySQL Database")]
```

---

## 4. Key Flows

### Login Flow
```mermaid
sequenceDiagram
    participant C as Client
    participant AC as AuthController
    participant AS as AuthService
    participant UM as UserManager
    participant DB as Database

    C->>AC: POST /auth/login {username, password}
    AC->>AS: LoginAsync(request)
    AS->>UM: FindByNameAsync(username)
    UM->>DB: SELECT from AspNetUsers
    DB-->>UM: ApplicationUser
    AS->>UM: CheckPasswordAsync(user, password)
    UM-->>AS: true / false
    AS->>AS: GenerateToken() → JWT with userId, username, role
    AS->>UM: SetAuthenticationTokenAsync (AccessToken)
    AS->>UM: SetAuthenticationTokenAsync (RefreshToken)
    UM->>DB: INSERT into AspNetUserTokens
    AS-->>AC: AuthResponse {jwt, refreshToken, username, isAdmin}
    AC-->>C: 200 OK {jwt, refreshToken, username, isAdmin}
```

---

### Checkout Flow
```mermaid
sequenceDiagram
    participant C as Client
    participant MW as TokenRevocationMiddleware
    participant BC as BooksController
    participant BS as BookService
    participant UoW as UnitOfWork
    participant DB as Database

    C->>MW: POST /api/books/5/checkout (Bearer JWT)
    MW->>DB: SELECT from AspNetUserTokens WHERE Value = jwt
    DB-->>MW: token found ✓
    MW->>BC: pass request
    BC->>BC: extract userId from JWT claims
    BC->>BS: CheckoutBookAsync(bookId:5, userId)
    BS->>UoW: UserBooks.CountActiveByUser(userId)
    UoW->>DB: SELECT COUNT active checkouts
    DB-->>BS: count=2 (under limit of 5 ✓)
    BS->>UoW: Reservations.GetActiveByUserAndBook(userId, 5)
    DB-->>BS: no reservation
    BS->>UoW: Books.GetAvailableCopyAsync(bookId:5)
    DB-->>BS: BookCopy {id:3, copyNumber:2}
    BS->>BS: physicalAvailable > pendingReservations? ✓
    BS->>UoW: mark copy.IsCheckedOut = true
    BS->>UoW: UserBooks.Add(new UserBook)
    BS->>UoW: SaveChangesAsync()
    UoW->>DB: UPDATE BookCopies + INSERT UserBooks (one transaction)
    DB-->>BS: success
    BS-->>BC: (true, null)
    BC-->>C: 200 OK "Book checked out. Due in 14 days."
```

---

### Reservation Flow
```mermaid
sequenceDiagram
    participant C as Client
    participant RC as ReservationsController
    participant RS as ReservationService
    participant UoW as UnitOfWork
    participant DB as Database

    C->>RC: POST /api/reservations/5
    RC->>RS: ReserveAsync(bookId:5, userId)
    RS->>UoW: Books.ExistsAsync(5)
    DB-->>RS: true ✓
    RS->>UoW: UserBooks.GetActiveByUserAndBook(userId, 5)
    DB-->>RS: null (not borrowed ✓)
    RS->>UoW: Reservations.GetActiveByUserAndBook(userId, 5)
    DB-->>RS: null (not in queue ✓)
    RS->>UoW: Reservations.Add(new Reservation)
    RS->>UoW: SaveChangesAsync()
    UoW->>DB: INSERT Reservations
    RS-->>RC: (true, null)
    RC-->>C: 200 OK {reservationId, queuePosition}
```

---

## 5. Database Schema

```mermaid
erDiagram
    ApplicationUser {
        int Id PK
        string UserName
        string PasswordHash
        bool IsAdmin
    }

    Book {
        int Id PK
        string Title
        string Author
        string Genre
        string Description
    }

    BookCopy {
        int Id PK
        int BookId FK
        int CopyNumber
        bool IsCheckedOut
        bool IsFaulty
        string FaultyReason
    }

    UserBook {
        int Id PK
        int UserId FK
        int BookId FK
        int BookCopyId FK
        DateTime CheckedOutAt
        DateTime DueDate
        DateTime ReturnedAt
    }

    Reservation {
        int Id PK
        int UserId FK
        int BookId FK
        DateTime ReservedAt
        DateTime FulfilledAt
        DateTime CancelledAt
    }

    Fine {
        int Id PK
        int UserId FK
        decimal Amount
        bool IsPaid
    }

    AuditLog {
        int Id PK
        int UserId FK
        string Action
        DateTime Timestamp
    }

    ApplicationUser ||--o{ UserBook : "borrows"
    ApplicationUser ||--o{ Reservation : "reserves"
    Book ||--o{ BookCopy : "has copies"
    Book ||--o{ UserBook : "borrowed via"
    Book ||--o{ Reservation : "reserved via"
    BookCopy ||--o{ UserBook : "tracked in"
```

---

## 6. Pattern Map

| Pattern | Where |
|---|---|
| **Clean Architecture** | 4 separate projects: `Domain` → `Application` → `Infrastructure` → `API` |
| **Repository Pattern** | `IBookRepository` (Application) implemented by `BookRepository` (Infrastructure) |
| **Unit of Work** | `IUnitOfWork` / `UnitOfWork` — one shared `AppDbContext`, one `SaveChangesAsync()` |
| **DbContext** | `AppDbContext` extends `IdentityDbContext` — EF Core gateway to MySQL |
| **ASP.NET Identity** | `ApplicationUser : IdentityUser<int>` — handles users, passwords, tokens |
| **Middleware** | `TokenRevocationMiddleware` — blocks revoked JWTs before hitting controllers |
| **Delegating Handler** | `LoggingDelegatingHandler` + `RetryDelegatingHandler` — wraps outbound HTTP calls |
| **MVC** | Controllers (C) + Domain Models (M) + JSON responses (V) |
