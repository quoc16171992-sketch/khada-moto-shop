using Demo1.Data;
using Demo1.Models;
using Demo1.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);
var railwayPort = Environment.GetEnvironmentVariable("PORT");
if (int.TryParse(railwayPort, out var port) && port > 0)
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

var persistentRoot = Environment.GetEnvironmentVariable("RAILWAY_VOLUME_MOUNT_PATH")
    ?? builder.Configuration["PersistentDataPath"];
if (!string.IsNullOrWhiteSpace(persistentRoot))
{
    persistentRoot = Path.GetFullPath(persistentRoot);
    Directory.CreateDirectory(persistentRoot);
}

var connectionString = !string.IsNullOrWhiteSpace(persistentRoot)
    ? $"Data Source={Path.Combine(persistentRoot, "khadashop.db")}" 
    : builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=khadashop.db";

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequiredLength = 12;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Admin/Login";
    options.AccessDeniedPath = "/Admin/Login";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
});

builder.Services.AddAuthorization(options =>
    options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin")));
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});
if (!string.IsNullOrWhiteSpace(persistentRoot))
{
    var keyPath = Path.Combine(persistentRoot, "keys");
    Directory.CreateDirectory(keyPath);
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(keyPath))
        .SetApplicationName("KhadaMotoShop");
}
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Admin", "AdminPolicy");
    options.Conventions.AllowAnonymousToPage("/Admin/Login");
});
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ISiteSettingsService, SiteSettingsService>();
builder.Services.AddScoped<IBannerService, BannerService>();
builder.Services.AddScoped<IImageService, LocalImageService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderCodeGenerator, OrderCodeGenerator>();
builder.Services.AddScoped<IOrderService, OrderService>();

var app = builder.Build();
app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseStatusCodePagesWithReExecute("/Error", "?httpStatusCode={0}");
var disabledOrderingPaths = new[]
{
    new PathString("/gio-hang"),
    new PathString("/dat-hang"),
    new PathString("/tra-cuu-don-hang"),
    new PathString("/Cart"),
    new PathString("/Checkout"),
    new PathString("/OrderTracking")
};
app.Use(async (context, next) =>
{
    if (disabledOrderingPaths.Any(path => context.Request.Path.StartsWithSegments(path, StringComparison.OrdinalIgnoreCase)))
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }

    await next();
});
app.UseHttpsRedirection();
app.UseStaticFiles();
if (!string.IsNullOrWhiteSpace(persistentRoot))
{
    var uploadPath = Path.Combine(persistentRoot, "uploads");
    Directory.CreateDirectory(uploadPath);
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(uploadPath),
        RequestPath = "/uploads"
    });
}
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

await using (var scope = app.Services.CreateAsyncScope())
{
    await DbInitializer.InitializeAsync(scope.ServiceProvider, app.Configuration);
}

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();
app.Run();

public partial class Program;
