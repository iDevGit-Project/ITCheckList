using ITCheckList.Models;
using ITCheckList.Models.Context;
using ITCheckList.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 💡 بارگذاری تنظیمات محیطی بر اساس ASPNETCORE_ENVIRONMENT
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables(); // (اختیاری) برای متغیرهای محیطی اگر لازم باشه

//builder.Configuration
//    .SetBasePath(Directory.GetCurrentDirectory())
//    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
//    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
//    .AddEnvironmentVariables();

// 🔧 افزودن سرویس‌ها
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ISystemMaintenanceService, SystemMaintenanceService>();
builder.Services.AddScoped<ICacheService, CacheService>();

// اتصال به دیتابیس
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("StrConDb")));

// حافظه موقت و کش
builder.Services.AddMemoryCache();
builder.Services.AddResponseCaching();


// DI سفارشی
builder.Services.AddScoped<TBL_LogEntry>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ILogService, LogService>();

// ConnectionStringDynamic متدهای
builder.Services.AddSingleton<IDatabaseConnectionService, DatabaseConnectionService>();
builder.Services.AddSingleton<ConnectionAttemptTracker>();


// Mapping متدهای
builder.Services.AddAutoMapper(typeof(MappingProfile));


// 📡 پشتیبانی از Reverse Proxy یا Nginx یا IIS
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

// 🔥 هندل کردن خطا بر اساس محیط
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseForwardedHeaders(); // 💡 قبل از Routing

app.UseRouting();
app.UseAuthorization();

// مسیرهای کنترلر
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "archive",
    pattern: "{controller=Checklist}/{action=ArchiveIndex}/{id?}");

app.Run();
