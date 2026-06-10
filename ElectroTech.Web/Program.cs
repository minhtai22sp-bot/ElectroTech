using Microsoft.AspNetCore.Authorization;
using ElectroTech.Web.Extensions;
using ElectroTech.Infrastructure.Extensions;
using Entities;
using Microsoft.AspNetCore.Identity;
using MediatR;
using ElectroTech.Web;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Http.Features;
using AspNetCoreHero.ToastNotification;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ElectroTech.Web.Permission;
using ElectroTech.Web.Abstractions;
using ElectroTech.Web.Service;
using System.Reflection;
using AspNetCoreHero.Extensions.Logging;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Identity.Web.UI;
using ElectroTech.Application.Features.Products.Queries;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using ElectroTech.Infrastructure.Repository;
using ElectroTech.Infrastructure.Services;
using Entities.ViewModel;
using Hangfire;
using Hangfire.SqlServer;
using Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ── Authorization ───────────────────────────────────────────────────────────────
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

// ── Common services ─────────────────────────────────────────────────────────────
builder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddTransient<IActionContextAccessor, ActionContextAccessor>();
builder.Services.AddScoped<IViewRenderService, ViewRenderService>();

// ── Infrastructure / DB ─────────────────────────────────────────────────────────
builder.Services.AddPersistenceContexts(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSharedInfrastructure(builder.Configuration);

// ── MVC / Razor ─────────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages()
    .AddMicrosoftIdentityUI();

// ── MediatR ─────────────────────────────────────────────────────────────────────
builder.Services.AddMediatR(
    typeof(Program).Assembly,
    typeof(GetAllPaginatedListQuery).Assembly,
    typeof(CartRepository).Assembly
);
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

// ── AutoMapper ──────────────────────────────────────────────────────────────────
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

// ── Notyf toast ─────────────────────────────────────────────────────────────────
builder.Services.AddNotyf(o =>
{
    o.DurationInSeconds = 5;
    o.IsDismissable = true;
    o.HasRippleEffect = true;
    o.Position = NotyfPosition.TopRight;
});

// ── Form options ────────────────────────────────────────────────────────────────
builder.Services.Configure<FormOptions>(options =>
{
    options.ValueCountLimit = 10000;
});

// ── Session ─────────────────────────────────────────────────────────────────────
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ── Repositories ────────────────────────────────────────────────────────────────
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
// ── Email ────────────────────────────────────────────────────────────────────────
builder.Services.Configure<MailSettings>(
    builder.Configuration.GetSection("MailSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();

// ── Hangfire ────────────────────────────────────────────────────────────────────
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(
        builder.Configuration.GetConnectionString("ApplicationConnection"),
        new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            QueuePollInterval = TimeSpan.Zero,
            UseRecommendedIsolationLevel = true,
            DisableGlobalLocks = true
        }));
builder.Services.AddHangfireServer();

// ── Identity options ────────────────────────────────────────────────────────────
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 3;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(3);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = false;
});

// ── Authentication ──────────────────────────────────────────────────────────────
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = new PathString("/Identity/Account/Login");
    options.LogoutPath = new PathString("/Identity/Account/Logout");
    options.AccessDeniedPath = new PathString("/Identity/Account/AccessDenied");
    options.Cookie.Name = "ElectroTech.AdminAuth";
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
})
.AddCookie(CookieAuthenticationCustomer.AuthenticationScheme, options =>
{
    options.LoginPath = new PathString("/Account/Login");
    options.LogoutPath = new PathString("/Account/Logout");
    options.AccessDeniedPath = new PathString("/Account/AccessDenied");
    options.Cookie.Name = "ElectroTech.CustomerAuth";
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(300);
    options.SlidingExpiration = true;
});

// ── Rate Limiting ───────────────────────────────────────────────────────────────
builder.Services.AddRateLimiter(o =>
{
    o.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 10;
    });
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// ── Logging ─────────────────────────────────────────────────────────────────────
builder.Host.UseSerilog();

// ═══════════════════════════════════════════════════════════════════════════════
var app = builder.Build();
// ═══════════════════════════════════════════════════════════════════════════════

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// ── Merge 2 cookie principals ────────────────────────────────────────────────────
app.Use(async (ctx, next) =>
{
    var principal = new ClaimsPrincipal();

    var customerResult = await ctx.AuthenticateAsync(
        CookieAuthenticationCustomer.AuthenticationScheme);
    if (customerResult?.Principal != null
        && customerResult.Principal.Identity?.IsAuthenticated == true)
        principal.AddIdentities(customerResult.Principal.Identities);

    var adminResult = await ctx.AuthenticateAsync(
        CookieAuthenticationDefaults.AuthenticationScheme);
    if (adminResult?.Principal != null
        && adminResult.Principal.Identity?.IsAuthenticated == true)
    {
        if (!principal.Identities.Any(i => i.IsAuthenticated))
            principal.AddIdentities(adminResult.Principal.Identities);
    }

    ctx.User = principal;
    await next();
});

// ── Seed DB ─────────────────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var loggerFactory = services.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger("app");
    try
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
        await ElectroTech.Infrastructure.Identity.Seeds.DefaultRoles
            .SeedAsync(userManager, roleManager);
        await ElectroTech.Infrastructure.Identity.Seeds.DefaultSuperAdminUser
            .SeedAsync(userManager, roleManager);
        logger.LogInformation("Application Starting");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "An error occurred seeding the DB");
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

// ── Hangfire Dashboard ───────────────────────────────────────────────────────────
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthFilter() }
});

app.MapRazorPages();
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}"
).RequireRateLimiting("fixed").RequireAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();