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
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];

        options.Scope.Add("profile");
        options.Scope.Add("email");

        options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
        options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
        options.ClaimActions.MapJsonKey("given_name", "given_name");
        options.ClaimActions.MapJsonKey("family_name", "family_name");
    });

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Shopkeeper/Login";
});

// =======================
// MVC
// =======================
builder.Services.AddControllersWithViews();

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

// =======================
// APPLY MIGRATIONS (SAFE)
// =======================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDBContext>();
    db.Database.Migrate();
}

// =======================
// FIREBASE (SAFE INIT)
// =======================
if (FirebaseApp.DefaultInstance == null)
{
    FirebaseApp.Create(new AppOptions
    {
        Credential = GoogleCredential.GetApplicationDefault()
    });
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
