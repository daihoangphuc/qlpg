using GymManagement.Web.Data;
using GymManagement.Web.Data.Repositories;
using GymManagement.Web.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add Entity Framework
builder.Services.AddDbContext<GymDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("GymDb")));

// Add Repository pattern
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<INguoiDungRepository, NguoiDungRepository>();
builder.Services.AddScoped<IGoiTapRepository, GoiTapRepository>();
builder.Services.AddScoped<ILopHocRepository, LopHocRepository>();

// Add Services
builder.Services.AddScoped<INguoiDungService, NguoiDungService>();
builder.Services.AddScoped<IGoiTapService, GoiTapService>();

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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<GymDbContext>();
    await DbInitializer.InitializeAsync(context);
}

app.Run();
