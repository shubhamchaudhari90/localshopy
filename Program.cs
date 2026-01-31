using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using localshopyNew.Data;
using localshopyNew.Services;
using localshopyNew.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// =======================
// DATABASE (SQLite)
// =======================
// Use a writable path on Azure Linux: /home/Data
var dbPath = "/home/Data";
if (!Directory.Exists(dbPath))
    Directory.CreateDirectory(dbPath);

builder.Configuration["ConnectionStrings:DefaultConnection"] = $"Data Source={Path.Combine(dbPath, "app.db")}";

builder.Services.AddDbContext<AppDBContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// =======================
// IDENTITY
// =======================
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDBContext>()
    .AddDefaultTokenProviders();

// =======================
// GOOGLE AUTH
// =======================
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        // OAuth client credentials from Google Cloud Console
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];

        // ---- Scopes ----
        // Required for OpenID Connect (user identity)
        options.Scope.Add("openid");

        // Basic profile info (name, picture, locale, etc.)
        options.Scope.Add("profile");

        // Email address and verification status
        options.Scope.Add("email");

        // ---- Claim mappings ----
        // Unique, stable identifier for the Google user
        options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "sub");

        // Primary email address
        options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");

        // Whether the email is verified by Google
        options.ClaimActions.MapJsonKey("email_verified", "email_verified");

        // Full display name (e.g., "John Doe")
        options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");

        // First name (given name)
        options.ClaimActions.MapJsonKey(ClaimTypes.GivenName, "given_name");

        // Last name (family name)
        options.ClaimActions.MapJsonKey(ClaimTypes.Surname, "family_name");

        // Profile photo URL
        options.ClaimActions.MapJsonKey("picture", "picture");
    });

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Shopkeeper/Login";
});

// =======================
// MVC
// =======================
builder.Services.AddControllersWithViews();

// ?? FIREBASE INIT — ONLY ONCE
if (FirebaseApp.DefaultInstance == null)
{
    var firebasePath = Path.Combine(AppContext.BaseDirectory, "firebase-service-account.json");

    FirebaseApp.Create(new AppOptions
    {
        Credential = GoogleCredential.FromFile(firebasePath)
    });
}
// =======================
// SESSION
// =======================
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// =======================
// DEPENDENCY INJECTION
// =======================
builder.Services.AddHttpContextAccessor();
builder.Services.AddDataProtection();
// Register your FirebaseNotificationService
builder.Services.AddScoped<FirebaseNotificationService>();

builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IShopkeeperService, ShopkeeperService>();
builder.Services.AddScoped<IShopService, ShopService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductMasterService, ProductMasterService>();
builder.Services.AddScoped<IShopProductService, ShopProductService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IEncodingService, EncodingService>();
builder.Services.AddScoped<IAdminService, AdminService>();

var app = builder.Build();

app.MapGet("/firebase-config.js", (IConfiguration config) =>
{
    var fcm = config.GetSection("Firebase");

    return Results.Text($@"
        window.firebaseConfig = {{
            apiKey: '{fcm["ApiKey"]}',
            authDomain: '{fcm["AuthDomain"]}',
            projectId: '{fcm["ProjectId"]}',
            storageBucket: '{fcm["StorageBucket"]}',
            messagingSenderId: '{fcm["MessagingSenderId"]}',
            appId: '{fcm["AppId"]}',
            vapidKey: '{fcm["VapidPublicKey"]}'
        }};
    ", "application/javascript");
});

// =======================
// APPLY MIGRATIONS (SAFE)
// =======================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDBContext>();
    db.Database.Migrate();
}

// =======================
// ERROR HANDLING + STATIC FILES
// =======================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseExceptionHandler("/Home/Error");
app.UseHsts();
app.UseExceptionHandler("/Error/500");
app.UseStatusCodePagesWithReExecute("/Error/{0}");

// =======================
// PIPELINE
// =======================
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// =======================
// ROUTING
// =======================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Customer}/{action=Location}/{id?}");

app.Run();
