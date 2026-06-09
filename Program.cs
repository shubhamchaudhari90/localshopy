using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using localshopyNew.Data;
using localshopyNew.Middleware;
using localshopyNew.Models;
using localshopyNew.Services;
using localshopyNew.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Diagnostics;
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
builder.Services.AddScoped<IBlockedUserService, BlockedUserService>();
builder.Services.AddScoped<ILogErrorsService, LogErrorsService>();


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

// Global Exception Handler
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionFeature = context.Features.Get<IExceptionHandlerPathFeature>();

        if (exceptionFeature?.Error != null)
        {
            try
            {
                using var scope = context.RequestServices.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDBContext>();

                var ex = exceptionFeature.Error;

                var log = new ErrorLog
                {
                    Message = ex.Message,
                    StackTrace = ex.StackTrace + " || " + ex.InnerException + "" + ex.Message,
                    Path = exceptionFeature.Path,
                    Method = context.Request.Method,
                    CreatedAt = DateTime.UtcNow
                };

                db.ErrorLogs.Add(log);
                await db.SaveChangesAsync();

                context.Response.Clear();
                context.Response.Redirect("/Account/Logout");
            }
            catch
            {
                // Avoid crashing if logging fails
            }
        }

        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsync("{\"error\":\"Internal Server Error\"}");
    });
});

// Optional: HTTPS redirection
app.UseHttpsRedirection();

//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Home/Error");
//    app.UseHsts();
//}

app.UseHttpsRedirection();

// Add middleware early in pipeline
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseStaticFiles();

app.UseStatusCodePages(async context =>
{
    var response = context.HttpContext.Response;

    if (response.StatusCode == StatusCodes.Status404NotFound)
    {
        context.HttpContext.Response.Redirect("/Account/Logout");
    }
});

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
