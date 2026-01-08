using localshopyNew.Data;
using localshopyNew.Services;
using localshopyNew.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Register SQLite DB 
builder.Services.AddDbContext<AppDBContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDBContext>()
    .AddDefaultTokenProviders();

// Google Authentication
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:client_id"];
        options.ClientSecret = builder.Configuration["Authentication:Google:client_secret"];

        options.Scope.Add("profile");
        options.Scope.Add("email");

        // Map claims

        options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
        options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
        options.ClaimActions.MapJsonKey("given_name", "given_name");
        options.ClaimActions.MapJsonKey("family_name", "family_name");



    });

// Redirect unauthorized users to Login
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Shopkeeper/Login";
});


// Add services to the container.
builder.Services.AddControllersWithViews();



// Add session services
builder.Services.AddDistributedMemoryCache(); // required for session storage
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IProductMasterService, ProductMasterService>();
builder.Services.AddScoped<IShopkeeperService, ShopkeeperService>();
builder.Services.AddScoped<IShopService, ShopService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IEncodingService, EncodingService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IShopProductService, ShopProductService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
if (!app.Environment.IsDevelopment())
    //{
    app.UseExceptionHandler("/Error/500");
//}

app.UseHttpsRedirection();
app.UseStaticFiles();

if (!app.Environment.IsDevelopment())
    //{
    app.UseStatusCodePagesWithReExecute("/Error/{0}");
//}


app.UseRouting();
app.UseSession();
app.UseAuthentication(); // Enables login
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Customer}/{action=Location}/{id?}");

app.Run();
