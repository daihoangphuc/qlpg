using GymManagement.Web.Data;
using GymManagement.Web.Data.Repositories;
using GymManagement.Web.Data.Identity;
using GymManagement.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add Entity Framework with Identity
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("GymDb")));

// Keep the original GymDbContext for backward compatibility
builder.Services.AddDbContext<GymDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("GymDb")));

// Add Identity
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;

    // Sign in settings
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure application cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.LoginPath = "/Auth/Login";
    options.LogoutPath = "/Auth/Logout";
    options.AccessDeniedPath = "/Auth/AccessDenied";
    options.SlidingExpiration = true;
});

// Add Repository pattern
builder.Services.AddScoped<IUnitOfWork>(provider =>
    new UnitOfWork(provider.GetRequiredService<ApplicationDbContext>()));
builder.Services.AddScoped<INguoiDungRepository>(provider =>
    new NguoiDungRepository(provider.GetRequiredService<ApplicationDbContext>()));
builder.Services.AddScoped<IGoiTapRepository>(provider =>
    new GoiTapRepository(provider.GetRequiredService<ApplicationDbContext>()));
builder.Services.AddScoped<ILopHocRepository>(provider =>
    new LopHocRepository(provider.GetRequiredService<ApplicationDbContext>()));

// Add Services
builder.Services.AddScoped<INguoiDungService, NguoiDungService>();
builder.Services.AddScoped<IGoiTapService, GoiTapService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Add HttpContextAccessor for AuthService
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Initialize database and Identity
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    // Initialize original gym database
    var gymContext = services.GetRequiredService<GymDbContext>();
    await DbInitializer.InitializeAsync(gymContext);

    // Initialize Identity
    var applicationContext = services.GetRequiredService<ApplicationDbContext>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

    await IdentityInitializer.InitializeAsync(userManager, roleManager, applicationContext);
}

app.Run();
