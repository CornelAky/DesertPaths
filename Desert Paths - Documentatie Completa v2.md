# Desert Paths - Documentație Tehnică Completă

## Cuprins

1. [Prezentare Generală](#1-prezentare-generală)
2. [Arhitectura Aplicației](#2-arhitectura-aplicației)
3. [Sistem de Roluri și Permisiuni](#3-sistem-de-roluri-și-permisiuni)
4. [Autentificare și Autorizare](#4-autentificare-și-autorizare)
5. [Modelele de Date](#5-modelele-de-date)
6. [Integrarea PayTabs](#6-integrarea-paytabs)
7. [Fluxuri de Utilizare](#7-fluxuri-de-utilizare)
8. [Structura Paginilor](#8-structura-paginilor)
9. [API Endpoints](#9-api-endpoints)
10. [Design Frontend](#10-design-frontend)
11. [Deployment](#11-deployment)

---

## 1. Prezentare Generală

### 1.1 Descrierea Proiectului

**Desert Paths Clone** este o platformă de turism pentru organizarea și rezervarea de excursii în deșert. Site-ul permite vizitatorilor să exploreze destinații, să selecteze tururi și să facă rezervări online cu plată integrată.

### 1.2 Obiective Principale

- ✅ Prezentarea atractivă a destinațiilor turistice (Lands)
- ✅ Afișarea tururilor disponibile cu detalii complete
- ✅ Sistem de rezervări online cu calendar
- ✅ Procesare plăți prin PayTabs
- ✅ Autentificare cu Google și Microsoft
- ✅ Sistem de roluri (Guest, Customer, Manager, Admin)
- ✅ Panou de administrare pentru gestionarea conținutului

### 1.3 Stack Tehnologic

| Layer | Tehnologii |
|-------|------------|
| **Framework** | .NET 10.0 LTS, ASP.NET Core MVC |
| **ORM** | Entity Framework Core |
| **Bază de date** | SQL Server |
| **Autentificare** | ASP.NET Core Identity + Google OAuth + Microsoft OAuth |
| **Plăți** | PayTabs Payment Gateway |
| **Frontend** | Tailwind CSS 3.x, Alpine.js, AOS, Swiper.js |
| **Email** | SendGrid / SMTP |

---

## 2. Arhitectura Aplicației

### 2.1 Structura Solution (Clean Architecture)

```
📁 DesertPaths.sln
├── 📁 src/
│   ├── 📁 DesertPaths.Domain/           # Entities, Enums, Interfaces
│   │   ├── 📁 Entities/
│   │   │   ├── Land.cs
│   │   │   ├── Journey.cs
│   │   │   ├── JourneyStyle.cs
│   │   │   ├── JourneyItinerary.cs
│   │   │   ├── Booking.cs
│   │   │   ├── Payment.cs
│   │   │   └── Review.cs
│   │   ├── 📁 Enums/
│   │   │   ├── BookingStatus.cs
│   │   │   ├── PaymentStatus.cs
│   │   │   └── DifficultyLevel.cs
│   │   └── 📁 Interfaces/
│   │
│   ├── 📁 DesertPaths.Application/      # Services, DTOs, Validators
│   │   ├── 📁 Interfaces/
│   │   ├── 📁 Services/
│   │   ├── 📁 DTOs/
│   │   └── 📁 Validators/
│   │
│   ├── 📁 DesertPaths.Infrastructure/   # Data Access, External Services
│   │   ├── 📁 Data/
│   │   │   ├── AppDbContext.cs
│   │   │   └── 📁 Configurations/
│   │   ├── 📁 Repositories/
│   │   ├── 📁 Services/
│   │   │   ├── PayTabsService.cs
│   │   │   └── EmailService.cs
│   │   └── 📁 Identity/
│   │
│   └── 📁 DesertPaths.Web/              # MVC Presentation Layer
│       ├── 📁 Areas/
│       │   └── 📁 Admin/
│       ├── 📁 Controllers/
│       ├── 📁 Views/
│       ├── 📁 ViewModels/
│       └── 📁 wwwroot/
│
└── 📁 tests/
    ├── 📁 DesertPaths.UnitTests/
    └── 📁 DesertPaths.IntegrationTests/
```

---

## 3. Sistem de Roluri și Permisiuni

### 3.1 Ierarhia Rolurilor

```
┌─────────────────────────────────────────────────────────────────┐
│                         GUEST                                   │
│  • Vizualizare site (Home, Lands, Journeys)                     │
│  • Pagina Contact                                               │
│  • Vizualizare recenzii                                         │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼ (se înregistrează automat)
┌─────────────────────────────────────────────────────────────────┐
│                        CUSTOMER                                 │
│  • Tot ce poate Guest                                           │
│  • + Creare rezervări                                           │
│  • + Plăți online                                               │
│  • + Scriere recenzii                                           │
│  • + Profil personal                                            │
│  • + Istoric rezervări proprii                                  │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼ (Admin promovează)
┌─────────────────────────────────────────────────────────────────┐
│                        MANAGER                                  │
│  • Tot ce poate Customer                                        │
│  • + Vizualiza TOATE rezervările                                │
│  • + Confirma/Anula rezervări                                   │
│  • + Vedea lista clienților                                     │
│  • + Adăuga/Edita Lands (destinații)                            │
│  • + Adăuga/Edita Journeys (tururi)                             │
│  • + Modera recenzii (aproba/respinge)                          │
│  • + Vedea rapoarte/statistici                                  │
│  • + Bloca utilizatori (Customer)                               │
│  ✗ NU poate șterge Lands/Journeys                               │
│  ✗ NU poate gestiona utilizatori (roluri)                       │
│  ✗ NU poate modifica setările site-ului                         │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼ (doar Admin poate crea)
┌─────────────────────────────────────────────────────────────────┐
│                         ADMIN                                   │
│  • Tot ce poate Manager                                         │
│  • + Șterge Lands/Journeys                                      │
│  • + Gestiona utilizatori (promovare, retrogradare)             │
│  • + Bloca utilizatori (Customer, Manager)                      │
│  • + Crea alți Admini                                           │
│  • + Modifica setările site-ului                                │
│  • + Acces complet la toate funcționalitățile                   │
└─────────────────────────────────────────────────────────────────┘
```

### 3.2 Tabel Permisiuni Detaliat

| Acțiune | Guest | Customer | Manager | Admin |
|---------|:-----:|:--------:|:-------:|:-----:|
| Vizualizare site public | ✅ | ✅ | ✅ | ✅ |
| Contact | ✅ | ✅ | ✅ | ✅ |
| Înregistrare/Login | ✅ | - | - | - |
| Creare rezervări | ❌ | ✅ | ✅ | ✅ |
| Plăți online | ❌ | ✅ | ✅ | ✅ |
| Scriere recenzii | ❌ | ✅ | ✅ | ✅ |
| Profil personal | ❌ | ✅ | ✅ | ✅ |
| Istoric rezervări proprii | ❌ | ✅ | ✅ | ✅ |
| Vizualiza TOATE rezervările | ❌ | ❌ | ✅ | ✅ |
| Confirma/Anula rezervări | ❌ | ❌ | ✅ | ✅ |
| Vedea lista clienților | ❌ | ❌ | ✅ | ✅ |
| Adăuga/Edita Lands | ❌ | ❌ | ✅ | ✅ |
| Adăuga/Edita Journeys | ❌ | ❌ | ✅ | ✅ |
| Șterge Lands/Journeys | ❌ | ❌ | ❌ | ✅ |
| Modera recenzii | ❌ | ❌ | ✅ | ✅ |
| Vedea rapoarte/statistici | ❌ | ❌ | ✅ | ✅ |
| Bloca Customer | ❌ | ❌ | ✅ | ✅ |
| Bloca Manager | ❌ | ❌ | ❌ | ✅ |
| Promovare Customer → Manager | ❌ | ❌ | ❌ | ✅ |
| Retrogradare Manager → Customer | ❌ | ❌ | ❌ | ✅ |
| Crea Admin | ❌ | ❌ | ❌ | ✅ |
| Setări site | ❌ | ❌ | ❌ | ✅ |

### 3.3 Fluxul de Promovare/Retrogradare

```
┌──────────────┐                    ┌──────────────┐
│   Customer   │◄───────────────────│   Manager    │
└──────┬───────┘   Retrogradare     └──────┬───────┘
       │           (doar Admin)            │
       │                                   │
       │         Promovare                 │
       └──────────(doar Admin)─────────────┘
```

### 3.4 Implementare Roluri în ASP.NET Core

```csharp
// Definire roluri
public static class AppRoles
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string Customer = "Customer";
    
    public static readonly string[] AllRoles = { Admin, Manager, Customer };
}

// Seed roluri la startup
public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
{
    foreach (var role in AppRoles.AllRoles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

// Atribuire rol la înregistrare (automat Customer)
public async Task<IActionResult> Register(RegisterViewModel model)
{
    var user = new ApplicationUser { ... };
    var result = await _userManager.CreateAsync(user, model.Password);
    
    if (result.Succeeded)
    {
        // Toți utilizatorii noi devin Customer automat
        await _userManager.AddToRoleAsync(user, AppRoles.Customer);
    }
}

// Promovare Customer → Manager (doar Admin)
[Authorize(Roles = "Admin")]
public async Task<IActionResult> PromoteToManager(string userId)
{
    var user = await _userManager.FindByIdAsync(userId);
    await _userManager.RemoveFromRoleAsync(user, AppRoles.Customer);
    await _userManager.AddToRoleAsync(user, AppRoles.Manager);
}
```

### 3.5 Authorization Policies

```csharp
// Program.cs
builder.Services.AddAuthorization(options =>
{
    // Politici simple
    options.AddPolicy("RequireAdmin", policy => 
        policy.RequireRole(AppRoles.Admin));
    
    options.AddPolicy("RequireManager", policy => 
        policy.RequireRole(AppRoles.Admin, AppRoles.Manager));
    
    options.AddPolicy("RequireCustomer", policy => 
        policy.RequireRole(AppRoles.Admin, AppRoles.Manager, AppRoles.Customer));
    
    // Politici specifice
    options.AddPolicy("CanManageContent", policy => 
        policy.RequireRole(AppRoles.Admin, AppRoles.Manager));
    
    options.AddPolicy("CanDeleteContent", policy => 
        policy.RequireRole(AppRoles.Admin));
    
    options.AddPolicy("CanManageUsers", policy => 
        policy.RequireRole(AppRoles.Admin));
    
    options.AddPolicy("CanBlockUsers", policy => 
        policy.RequireRole(AppRoles.Admin, AppRoles.Manager));
});
```

---

## 4. Autentificare și Autorizare

### 4.1 Strategia de Autentificare

Aplicația folosește **ASP.NET Core Identity** cu **External Login Providers**:

- ✅ Email + Parolă (cont local)
- ✅ Google OAuth 2.0
- ✅ Microsoft Account

### 4.2 Diagrama Fluxului de Autentificare

```
┌─────────────────────────────────────────────────────────────────┐
│                  OPȚIUNI DE AUTENTIFICARE                       │
└─────────────────────────────────────────────────────────────────┘
                              │
        ┌─────────────────────┼─────────────────────┐
        ▼                     ▼                     ▼
   ┌─────────┐          ┌──────────┐          ┌─────────┐
   │  EMAIL  │          │  GOOGLE  │          │MICROSOFT│
   │ + PASS  │          │  OAuth   │          │  OAuth  │
   └────┬────┘          └────┬─────┘          └────┬────┘
        │                    │                     │
        └────────────────────┼─────────────────────┘
                             ▼
                    ┌─────────────────┐
                    │  ASP.NET Core   │
                    │    Identity     │
                    └────────┬────────┘
                             │
                    ┌────────▼────────┐
                    │  Atribuire Rol  │
                    │   (Customer)    │
                    └────────┬────────┘
                             │
                    ┌────────▼────────┐
                    │   User Profile  │
                    │   + Bookings    │
                    └─────────────────┘
```

### 4.3 Pachete NuGet Necesare

```xml
<!-- Authentication -->
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="10.0.x" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.Google" Version="10.0.x" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.MicrosoftAccount" Version="10.0.x" />
```

### 4.4 Configurare Program.cs

```csharp
// ===== IDENTITY CONFIGURATION =====
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// ===== EXTERNAL AUTHENTICATION PROVIDERS =====
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
        options.CallbackPath = "/signin-google";
    })
    .AddMicrosoftAccount(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Microsoft:ClientId"]!;
        options.ClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"]!;
        options.CallbackPath = "/signin-microsoft";
    });
```

### 4.5 Model ApplicationUser Extins

```csharp
public class ApplicationUser : IdentityUser
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? Country { get; set; }
    
    public string? ProfileImageUrl { get; set; }
    
    public bool IsBlocked { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Computed Property
    public string FullName => $"{FirstName} {LastName}";
    
    // Navigation Properties
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
```

### 4.6 Configurare appsettings.json

```json
{
  "Authentication": {
    "Google": {
      "ClientId": "YOUR_GOOGLE_CLIENT_ID.apps.googleusercontent.com",
      "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET"
    },
    "Microsoft": {
      "ClientId": "YOUR_MICROSOFT_CLIENT_ID",
      "ClientSecret": "YOUR_MICROSOFT_CLIENT_SECRET"
    }
  }
}
```

### 4.7 Configurare Google OAuth

**Pași pentru Google Cloud Console:**

1. Accesează https://console.cloud.google.com/
2. Creează un proiect nou sau selectează unul existent
3. Navighează la **APIs & Services** → **Credentials**
4. Click **Create Credentials** → **OAuth 2.0 Client IDs**
5. Selectează **Web application**
6. Adaugă **Authorized redirect URIs**:
   - Development: `https://localhost:5001/signin-google`
   - Production: `https://yourdomain.com/signin-google`
7. Copiază **Client ID** și **Client Secret**

### 4.8 Configurare Microsoft OAuth

**Pași pentru Microsoft Entra Admin Center:**

1. Accesează https://entra.microsoft.com/
2. Navighează la **Applications** → **App registrations**
3. Click **New registration**
4. Configurează:
   - Name: `Desert Paths`
   - Supported account types: **Accounts in any organizational directory and personal Microsoft accounts**
5. Adaugă **Redirect URIs**:
   - Development: `https://localhost:5001/signin-microsoft`
   - Production: `https://yourdomain.com/signin-microsoft`
6. Generează **Client Secret** în **Certificates & secrets**

---

## 5. Modelele de Date

### 5.1 Diagrama ERD

```
┌─────────────────────┐       ┌─────────────────────┐
│        Land         │       │    JourneyStyle     │
├─────────────────────┤       ├─────────────────────┤
│ Id                  │       │ Id                  │
│ Name                │       │ Name                │
│ Slug                │       │ Description         │
│ Description         │       │ PriceMultiplier     │
│ HeroImageUrl        │       └──────────┬──────────┘
│ ThumbnailUrl        │                  │
│ DisplayOrder        │                  │ 1:N
│ IsActive            │                  │
└──────────┬──────────┘                  │
           │                             │
           │ 1:N                         │
           ▼                             ▼
┌───────────────────────────────────────────────────┐
│                     Journey                        │
├───────────────────────────────────────────────────┤
│ Id, Title, Slug, LandId, DefaultStyleId           │
│ ShortDescription, FullDescription                 │
│ DurationDays, DurationNights                      │
│ PriceFrom, MaxGroupSize, DifficultyLevel          │
│ HeroImageUrl, IsFeatured, IsActive                │
└────────────────────────┬──────────────────────────┘
                         │
           ┌─────────────┼─────────────┐
           │             │             │
           ▼             ▼             ▼
    ┌────────────┐ ┌───────────┐ ┌────────────┐
    │ Itinerary  │ │  Images   │ │ Highlights │
    └────────────┘ └───────────┘ └────────────┘
                         │
                         │ 1:N
                         ▼
              ┌─────────────────────┐
              │       Booking       │
              ├─────────────────────┤
              │ Id                  │
              │ BookingReference    │
              │ JourneyId           │
              │ UserId ◄────────────┼──── ApplicationUser
              │ StyleId             │
              │ TravelDate          │
              │ NumberOfGuests      │
              │ TotalPrice          │
              │ Status              │
              └──────────┬──────────┘
                         │
                         │ 1:1
                         ▼
              ┌─────────────────────┐
              │       Payment       │
              ├─────────────────────┤
              │ Id                  │
              │ BookingId           │
              │ TransactionRef      │
              │ Amount              │
              │ Currency            │
              │ Status              │
              │ PayTabsResponse     │
              └─────────────────────┘
```

### 5.2 Entitățile C#

#### Land.cs
```csharp
public class Land
{
    public int Id { get; set; }
    
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required, MaxLength(100)]
    public string Slug { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? ShortDescription { get; set; }
    
    public string? FullDescription { get; set; }
    
    public string? HeroImageUrl { get; set; }
    
    public string? ThumbnailUrl { get; set; }
    
    public int DisplayOrder { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation
    public virtual ICollection<Journey> Journeys { get; set; } = new List<Journey>();
}
```

#### Journey.cs
```csharp
public class Journey
{
    public int Id { get; set; }
    
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required, MaxLength(200)]
    public string Slug { get; set; } = string.Empty;
    
    public int LandId { get; set; }
    
    public int DefaultStyleId { get; set; }
    
    [MaxLength(500)]
    public string? ShortDescription { get; set; }
    
    public string? FullDescription { get; set; }
    
    public int DurationDays { get; set; }
    
    public int DurationNights { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal PriceFrom { get; set; }
    
    public int MaxGroupSize { get; set; } = 20;
    
    public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Moderate;
    
    public string? HeroImageUrl { get; set; }
    
    public bool IsFeatured { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation
    public virtual Land Land { get; set; } = null!;
    public virtual JourneyStyle DefaultStyle { get; set; } = null!;
    public virtual ICollection<JourneyItinerary> Itineraries { get; set; } = new List<JourneyItinerary>();
    public virtual ICollection<JourneyImage> Images { get; set; } = new List<JourneyImage>();
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
```

#### Booking.cs
```csharp
public class Booking
{
    public int Id { get; set; }
    
    [Required, MaxLength(20)]
    public string BookingReference { get; set; } = string.Empty;
    
    public int JourneyId { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    public int StyleId { get; set; }
    
    public DateTime TravelDate { get; set; }
    
    public int NumberOfGuests { get; set; } = 1;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPrice { get; set; }
    
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    
    [MaxLength(1000)]
    public string? SpecialRequests { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation
    public virtual Journey Journey { get; set; } = null!;
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual JourneyStyle Style { get; set; } = null!;
    public virtual Payment? Payment { get; set; }
}
```

#### Payment.cs
```csharp
public class Payment
{
    public int Id { get; set; }
    
    public int BookingId { get; set; }
    
    [MaxLength(100)]
    public string? TransactionReference { get; set; }
    
    [MaxLength(100)]
    public string? PayTabsTransactionId { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }
    
    [MaxLength(3)]
    public string Currency { get; set; } = "SAR";
    
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    
    public string? PayTabsResponseJson { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? PaidAt { get; set; }
    
    // Navigation
    public virtual Booking Booking { get; set; } = null!;
}
```

### 5.3 Enums

```csharp
public enum BookingStatus
{
    Pending = 0,
    Confirmed = 1,
    Completed = 2,
    Cancelled = 3
}

public enum PaymentStatus
{
    Pending = 0,
    Success = 1,
    Failed = 2,
    Refunded = 3
}

public enum DifficultyLevel
{
    Easy = 0,
    Moderate = 1,
    Challenging = 2,
    Expert = 3
}
```

---

## 6. Integrarea PayTabs

### 6.1 Configurare appsettings.json

```json
{
  "PayTabs": {
    "ProfileId": "YOUR_PROFILE_ID",
    "ServerKey": "YOUR_SERVER_KEY",
    "BaseUrl": "https://secure.paytabs.sa",
    "Currency": "SAR",
    "ReturnUrl": "https://yourdomain.com/booking/payment/return",
    "CallbackUrl": "https://yourdomain.com/booking/payment/callback"
  }
}
```

### 6.2 PayTabs Service

```csharp
public interface IPayTabsService
{
    Task<PaymentInitResponse> InitiatePaymentAsync(Booking booking);
    Task<PaymentVerifyResponse> VerifyPaymentAsync(string transactionRef);
    Task HandleCallbackAsync(PayTabsCallback callback);
}

public class PayTabsService : IPayTabsService
{
    private readonly HttpClient _httpClient;
    private readonly PayTabsSettings _settings;

    public async Task<PaymentInitResponse> InitiatePaymentAsync(Booking booking)
    {
        var request = new
        {
            profile_id = _settings.ProfileId,
            tran_type = "sale",
            tran_class = "ecom",
            cart_id = booking.BookingReference,
            cart_description = $"Booking: {booking.Journey.Title}",
            cart_currency = _settings.Currency,
            cart_amount = booking.TotalPrice,
            callback = _settings.CallbackUrl,
            @return = _settings.ReturnUrl,
            customer_details = new
            {
                name = booking.User.FullName,
                email = booking.User.Email,
                phone = booking.User.PhoneNumber ?? "",
                country = booking.User.Country ?? "SA"
            }
        };

        var response = await _httpClient.PostAsJsonAsync("/payment/request", request);
        return await response.Content.ReadFromJsonAsync<PaymentInitResponse>();
    }
}
```

### 6.3 Fluxul de Plată

```
┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│   Booking    │────▶│   Payment    │────▶│   PayTabs    │
│    Form      │     │   Initiate   │     │   Redirect   │
└──────────────┘     └──────────────┘     └──────┬───────┘
                                                  │
                                                  ▼
                                          ┌──────────────┐
                                          │   Customer   │
                                          │ Enters Card  │
                                          └──────┬───────┘
                                                  │
                     ┌────────────────────────────┼────────────────────────────┐
                     │                            │                            │
                     ▼                            ▼                            ▼
              ┌──────────────┐           ┌──────────────┐           ┌──────────────┐
              │   SUCCESS    │           │    FAILED    │           │  CANCELLED   │
              └──────┬───────┘           └──────────────┘           └──────────────┘
                     │
                     ▼
              ┌──────────────┐
              │ Confirmation │
              │   + Email    │
              └──────────────┘
```

---

## 7. Fluxuri de Utilizare

### 7.1 Flux Complet de Rezervare

```
1. Vizitator explorează site-ul
           │
           ▼
2. Selectează o destinație (Land)
           │
           ▼
3. Vizualizează tururile disponibile
           │
           ▼
4. Selectează un tur (Journey)
           │
           ▼
5. Click "Book Now" → Verificare autentificare
           │
     ┌─────┴─────┐
     │           │
     ▼           ▼
  Logat?      Nu e logat?
     │           │
     │           ▼
     │      Login/Register
     │      (Google/Microsoft/Email)
     │           │
     │           ▼
     │      Devine Customer automat
     │           │
     └─────┬─────┘
           │
           ▼
6. Completează formularul de rezervare
           │
           ▼
7. Review & Confirm
           │
           ▼
8. Redirect la PayTabs → Plată
           │
           ▼
9. Confirmare + Email
```

### 7.2 Status-uri Booking

```
┌──────────┐    ┌───────────┐    ┌───────────┐
│ PENDING  │───▶│ CONFIRMED │───▶│ COMPLETED │
└────┬─────┘    └───────────┘    └───────────┘
     │
     ▼
┌───────────┐
│ CANCELLED │
└───────────┘
```

---

## 8. Structura Paginilor

### 8.1 Pagini Publice (Guest + Customer)

| Pagină | URL | Acces |
|--------|-----|-------|
| Home | `/` | Toți |
| Lands | `/lands` | Toți |
| Land Detail | `/lands/{slug}` | Toți |
| Journeys | `/journeys` | Toți |
| Journey Detail | `/journeys/{slug}` | Toți |
| Our Story | `/our-story` | Toți |
| Contact | `/contact` | Toți |

### 8.2 Pagini Autentificare

| Pagină | URL | Acces |
|--------|-----|-------|
| Login | `/account/login` | Guest |
| Register | `/account/register` | Guest |
| Profile | `/account/profile` | Customer+ |
| My Bookings | `/account/bookings` | Customer+ |

### 8.3 Pagini Booking

| Pagină | URL | Acces |
|--------|-----|-------|
| Booking Form | `/booking/{slug}` | Customer+ |
| Payment | `/booking/payment/{id}` | Customer+ |
| Confirmation | `/booking/confirmation/{id}` | Customer+ |

### 8.4 Admin Area

| Pagină | URL | Manager | Admin |
|--------|-----|:-------:|:-----:|
| Dashboard | `/admin` | ✅ | ✅ |
| Lands - List | `/admin/lands` | ✅ | ✅ |
| Lands - Create/Edit | `/admin/lands/create` | ✅ | ✅ |
| Lands - Delete | `/admin/lands/delete/{id}` | ❌ | ✅ |
| Journeys - List | `/admin/journeys` | ✅ | ✅ |
| Journeys - Create/Edit | `/admin/journeys/create` | ✅ | ✅ |
| Journeys - Delete | `/admin/journeys/delete/{id}` | ❌ | ✅ |
| Bookings | `/admin/bookings` | ✅ | ✅ |
| Customers | `/admin/customers` | ✅ | ✅ |
| Block User | `/admin/users/block/{id}` | ✅* | ✅ |
| Reviews | `/admin/reviews` | ✅ | ✅ |
| Users Management | `/admin/users` | ❌ | ✅ |
| Settings | `/admin/settings` | ❌ | ✅ |

*Manager poate bloca doar Customer, nu și alți Manageri

---

## 9. API Endpoints

### 9.1 Public Routes

```
GET  /                          → Home/Index
GET  /lands                     → Lands/Index
GET  /lands/{slug}              → Lands/Details
GET  /journeys                  → Journeys/Index
GET  /journeys/{slug}           → Journeys/Details
GET  /our-story                 → Home/OurStory
GET  /contact                   → Contact/Index
POST /contact                   → Contact/Submit
```

### 9.2 Auth Routes

```
GET  /account/login             → Account/Login
POST /account/login             → Account/Login
GET  /account/register          → Account/Register
POST /account/register          → Account/Register (devine Customer)
POST /account/external-login    → Account/ExternalLogin
GET  /account/external-callback → Account/ExternalLoginCallback
POST /account/logout            → Account/Logout
GET  /account/profile           → Account/Profile
```

### 9.3 Booking Routes (Customer+)

```
GET  /booking/{slug}            → Booking/Create
POST /booking/create            → Booking/Create
GET  /booking/payment/{id}      → Booking/Payment
POST /booking/payment/callback  → Booking/PayTabsCallback
GET  /booking/confirmation/{id} → Booking/Confirmation
GET  /account/bookings          → Booking/MyBookings
```

### 9.4 Admin Routes (Manager/Admin)

```
GET  /admin                     → [Manager, Admin]
GET  /admin/lands               → [Manager, Admin]
POST /admin/lands/create        → [Manager, Admin]
POST /admin/lands/edit/{id}     → [Manager, Admin]
POST /admin/lands/delete/{id}   → [Admin only]
GET  /admin/bookings            → [Manager, Admin]
POST /admin/bookings/confirm    → [Manager, Admin]
POST /admin/bookings/cancel     → [Manager, Admin]
GET  /admin/users               → [Admin only]
POST /admin/users/promote       → [Admin only]
POST /admin/users/block         → [Manager*, Admin]
GET  /admin/settings            → [Admin only]
```

---

## 10. Design Frontend

### 10.1 Stack Frontend

- **Tailwind CSS 3.x** - Styling
- **Alpine.js** - Interactivitate ușoară
- **AOS** - Animații la scroll
- **Swiper.js** - Carousele
- **Leaflet.js** - Hărți interactive

### 10.2 Paleta de Culori

```css
:root {
  --color-sand-50: #fefce8;
  --color-sand-500: #eab308;
  --color-sand-700: #a16207;
  --color-desert-500: #c2410c;
  --color-desert-700: #7c2d12;
}
```

---

## 11. Deployment

### 11.1 Variabile de Mediu (Production)

```bash
# Database
ConnectionStrings__DefaultConnection="Server=...;Database=DesertPaths;..."

# Authentication
Authentication__Google__ClientId="..."
Authentication__Google__ClientSecret="..."
Authentication__Microsoft__ClientId="..."
Authentication__Microsoft__ClientSecret="..."

# PayTabs
PayTabs__ProfileId="..."
PayTabs__ServerKey="..."

# Email
SendGrid__ApiKey="..."
```

### 11.2 Opțiuni de Hosting

- **Azure App Service** (recomandat pentru .NET)
- **AWS Elastic Beanstalk**
- **DigitalOcean App Platform**

---

## 12. Checklist Implementare

- [ ] Setup proiect ASP.NET Core MVC
- [ ] Configurare Tailwind CSS
- [ ] Implementare Entities și DbContext
- [ ] Setup ASP.NET Core Identity
- [ ] Configurare Google OAuth
- [ ] Configurare Microsoft OAuth
- [ ] Implementare sistem de roluri
- [ ] Creare Controllers și Views publice
- [ ] Creare Admin Area
- [ ] Integrare PayTabs
- [ ] Implementare email notifications
- [ ] Testing
- [ ] Deployment

---

*Documentație generată pentru proiectul Desert Paths Clone*
*Versiune: 2.0 | Data: Februarie 2026*