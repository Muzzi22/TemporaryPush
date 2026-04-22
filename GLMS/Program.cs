using GLMS.Web.Data;
using GLMS.Web.Models;
using GLMS.Web.Patterns.Factory;
using GLMS.Web.Patterns.Observer;
using GLMS.Web.Patterns.Repository;
using GLMS.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── MVC ──────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

// ── EF Core ──────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Identity ─────────────────────────────────────────────────
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// ── Cookie settings ───────────────────────────────────────────
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// ── Repository Pattern ────────────────────────────────────────
builder.Services.AddScoped<IContractRepository, ContractRepository>();
builder.Services.AddScoped<IServiceRequestRepository, ServiceRequestRepository>();

// ── Factory Pattern ───────────────────────────────────────────
builder.Services.AddScoped<IContractFactory, StandardContractFactory>();

// ── Observer Pattern ──────────────────────────────────────────
builder.Services.AddSingleton<ContractEventService>(provider =>
{
    var eventService = new ContractEventService();
    var logger = provider.GetRequiredService<ILogger<ContractExpiryObserver>>();
    eventService.Subscribe(new ContractExpiryObserver(logger));
    return eventService;
});

// ── Services ──────────────────────────────────────────────────
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddHttpClient<ICurrencyService, CurrencyService>();
builder.Services.AddScoped<IContractPdfService, ContractPdfService>();
builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();

// ── Seed database on startup ──────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
    await DbSeeder.SeedRolesAndAdminAsync(scope.ServiceProvider);
}

// ── HTTP pipeline ─────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();