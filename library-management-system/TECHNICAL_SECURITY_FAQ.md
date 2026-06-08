# Technical & Security Q&A

## Technical Questions

### 1. Why .NET + React?

**Answer:**
- **.NET 10**: Modern, statically-typed, high-performance backend with built-in dependency injection, Entity Framework for ORM, and excellent async/await support. Ideal for business logic and data access layers.
- **React**: Industry-standard SPA framework with ecosystem (React Router, Axios). Enables fast frontend iteration and component reusability.
- **Combined**: Clean API-first separation of concerns — backend exposes REST endpoints, frontend consumes them independently.

---

### 2. Is the architecture scalable?

**Current State:**
- ✅ Layered architecture (Controllers → Services → Data) — easy to add caching, queuing, or additional services.
- ✅ DTOs separate API contracts from domain models — breaking changes won't leak to DB.
- ✅ EF Core with async/await — can handle concurrent requests.

**Bottlenecks:**
- ❌ Single MySQL instance — no sharding or read replicas.
- ❌ JWT stored in localStorage — no server-side invalidation (logout takes 7 days to expire).
- ❌ No caching layer (Redis) — repeated queries hit DB every time.
- ❌ No load balancing or horizontal scaling setup.

**Recommendation:** Add Redis for session/cache, implement distributed tracing, and containerize for Kubernetes deployment when scaling beyond ~10K concurrent users.

---

### 3. Why is there no testing code in the repo?

**Answer:**
The project lacks unit, integration, and e2e tests. 

**Risks:**
- Refactoring breaks existing features silently.
- Checkout logic, auth, reservation rules aren't validated.
- New hires can't safely modify code.

**Recommendation:** Add xUnit tests for BookService, AuthService, and ReservationService. Aim for 70%+ coverage on business logic.

---

### 4. How is error handling done?

**Current State:**
- Tuple returns like `(bool Success, string? Error)` for checkout/return.
- HTTP status codes (200, 400, 404) are inferred from controller logic.

**Issues:**
- No centralized error logging.
- No structured error responses (clients can't parse error types).
- Exceptions aren't caught globally.

**Recommendation:** Add a global exception middleware to log errors, return consistent JSON error format (`{ "error": "...", "code": "CHECKOUT_LIMIT_REACHED" }`), and track errors in a log aggregator.

---

### 5. How does the admin role work?

**Answer:**
- Stored as `User.IsAdmin` boolean.
- JWT includes `ClaimTypes.Role` set to "Admin" or "User".
- `AdminRoute` component on frontend checks `user.isAdmin`.
- Backend controllers don't enforce role-based authorization yet — `AdminController` isn't gated by `[Authorize(Roles = "Admin")]`.

**Risk:** Frontend check alone is unsafe; a malicious client could forge a JWT or modify localStorage to claim admin role.

**Recommendation:** Add `[Authorize(Roles = "Admin")]` attribute to all admin endpoints in the backend.

---

## Security Questions

### 6. Why is SHA-256 used for passwords instead of bcrypt or Argon2?

**Current Implementation:**
```csharp
private static string HashPassword(string password)
{
    using var sha256 = SHA256.Create();
    var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
    return Convert.ToBase64String(hash);
}
```

**Why this is bad:**
- ❌ SHA-256 is a **fast hash** — brute-force attacks are trivial (billions of attempts/sec on modern GPUs).
- ❌ No salt — identical passwords produce identical hashes (rainbow tables work).
- ❌ No iteration count — attackers don't pay a computational penalty.

**Recommendation:**
Replace with **bcrypt** or **Argon2** (adaptive hashing). Example with bcrypt:

```csharp
// Install: NuGet package BCrypt.Net-Next
private static string HashPassword(string password)
{
    return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
}

private static bool VerifyPassword(string password, string hash)
{
    return BCrypt.Net.BCrypt.Verify(password, hash);
}
```

---

### 7. Is JWT implementation secure?

**Current Setup:**
- 7-day expiry, no refresh tokens.
- Token stored in `localStorage`.
- Interceptor attaches `Bearer` token on every request.

**Issues:**
- ❌ **localStorage is vulnerable to XSS** — any malicious script on the page can steal the token.
- ❌ **7-day expiry is too long** — stolen token grants access for a week.
- ❌ **No refresh token rotation** — can't revoke active sessions without waiting 7 days.

**Recommendation:**
1. Move token to **httpOnly cookie** (immune to XSS).
2. Implement **refresh token rotation**: short-lived access token (15 min) + longer-lived refresh token (7 days).
3. On logout, invalidate refresh token in DB or blacklist.

Example flow:
```
Login → server returns: AccessToken (15 min, httpOnly), RefreshToken (7 days, httpOnly)
Request → AttachAccessToken
AccessToken expires → call /refresh with RefreshToken → new AccessToken
Logout → invalidate RefreshToken in DB
```

---

### 8. Is CORS configuration safe?

**Current Setup in Program.cs:**
```csharp
options.AddPolicy("AllowReact", policy =>
    policy.WithOrigins("http://localhost:5173")
          .AllowAnyHeader()
          .AllowAnyMethod());
```

**Issues:**
- ✅ Hardcoded to localhost:5173 (good for dev).
- ⚠️ `AllowAnyHeader()` + `AllowAnyMethod()` are broad — attackers can make any request from an allowed origin.
- ❌ In production, if origin whitelist is wrong, API is exposed or completely blocked.

**Recommendation (Production):**
```csharp
policy.WithOrigins("https://library.example.com")
      .AllowCredentials() // for cookies
      .WithMethods("GET", "POST", "PUT", "DELETE")
      .WithHeaders("Authorization", "Content-Type");
```

---

### 9. What about input validation?

**Current State:**
DTOs have no validation attributes. Controllers accept raw input and pass to services.

Example:
```csharp
public async Task<IActionResult> Register([FromBody] RegisterRequest request)
{
    // No check if request.Username is empty, password too short, etc.
    return Ok(await _authService.RegisterAsync(request));
}
```

**Risks:**
- ❌ SQL injection via malicious strings (though EF Core parameterizes queries).
- ❌ Empty/null usernames, weak passwords accepted.
- ❌ No rate limiting on auth endpoints (brute force attacks).

**Recommendation:**
```csharp
public record RegisterRequest(
    [Required] 
    [StringLength(50, MinimumLength = 3)]
    string Username,
    
    [Required]
    [StringLength(100, MinimumLength = 8)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)")]
    string Password // must have lowercase, uppercase, digit
);
```

Add `[ApiController]` to controllers for automatic validation.

---

### 10. Does the API have rate limiting?

**Current State:** No.

**Risk:**
- Brute-force login attacks: attacker tries 1 million passwords/minute.
- DDoS: attacker floods `/books` with requests.
- Reservation spam: user reserves same book 10K times.

**Recommendation:**
Use **AspNetCoreRateLimit** NuGet package:

```csharp
builder.Services.AddMemoryCache();
builder.Services.AddInMemoryRateLimiting();

app.UseIpRateLimiting();

// appsettings.json:
"IpRateLimitPolicies": {
  "default": [
    {
      "endpoint": "*:/auth/*",
      "limit": 5,
      "period": "1m" // 5 requests per minute for auth
    },
    {
      "endpoint": "*:/books/*",
      "limit": 100,
      "period": "1m"
    }
  ]
}
```

---

### 11. Is the database connection secure?

**Current Setup (appsettings.json):**
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=library_db;User=root;Password=;"
}
```

**Issues:**
- ❌ **Empty password** for MySQL root user — not secure.
- ❌ **Credentials in appsettings.json** — if code is leaked, DB access is compromised.
- ❌ **No SSL connection to DB** — traffic is unencrypted.

**Recommendation:**
1. Use a dedicated DB user with strong password and limited privileges.
2. Store credentials in **environment variables** or **Azure Key Vault**:
   ```csharp
   var connString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
   ```
3. Enable SSL on the connection:
   ```json
   "DefaultConnection": "Server=localhost;Database=library_db;User=libuser;Password=secure_password;SslMode=Required;"
   ```

---

### 12. What about SQL injection?

**Current State:** ✅ Safe.

**Why:**
- EF Core uses parameterized queries under the hood.
- No raw SQL strings concatenating user input.

Example (safe):
```csharp
var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
// Compiles to: SELECT * FROM Users WHERE Username = @Username;
```

**Recommendation:** Keep using EF Core. Avoid `.FromSqlRaw()` with string concatenation.

---

### 13. Is sensitive data leaking?

**Current State:**
- ✅ Password hashes never sent to frontend.
- ✅ DTOs filter which fields are exposed.
- ⚠️ But weak hashing makes hashes vulnerable if DB is breached.

**Recommendation:**
- Fix password hashing (bcrypt/Argon2).
- Add audit logging: track who viewed what data.
- Encrypt PII fields at rest if required by compliance.

---

### 14. What's the disaster recovery plan?

**Current State:** None documented.

**Questions:**
- How often is the DB backed up?
- Where are backups stored?
- How long to restore from a backup?
- Is there a failover replica?

**Recommendation:**
- Daily automated backups to cloud storage (AWS S3, Azure Blob).
- Test restore procedure monthly.
- Document RTO (Recovery Time Objective) and RPO (Recovery Point Objective).

---

## Summary Table

| Area | Status | Priority |
|------|--------|----------|
| Password Hashing | ❌ SHA-256 (weak) | 🔴 **Critical** |
| JWT Security | ⚠️ localStorage, no refresh | 🔴 **Critical** |
| Role-Based Access | ⚠️ Frontend only | 🔴 **Critical** |
| Input Validation | ❌ Missing | 🟠 **High** |
| Rate Limiting | ❌ Missing | 🟠 **High** |
| Error Logging | ❌ None | 🟡 **Medium** |
| Testing | ❌ None | 🟡 **Medium** |
| DB Credentials | ❌ In code | 🟠 **High** |
| CORS | ✅ Dev-only | ✅ Good |
| SQL Injection | ✅ EF Core safe | ✅ Good |

