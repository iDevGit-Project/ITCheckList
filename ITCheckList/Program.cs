﻿using ITCheckList.Models;
using ITCheckList.Models.Context;
using ITCheckList.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// اتصال به دیتابیس
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("StrConDb")));

// در متد ConfigureServices یا قبل از Build
builder.Services.AddMemoryCache();         // فعال کردن In-Memory Cache
builder.Services.AddResponseCaching();     // فعال کردن Response Caching
builder.Services.AddScoped<TBL_LogEntry>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ILogService, LogService>();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

    // اگر در محیط داخلی هستی و فقط از localhost استفاده می‌کنی، این خط را باز کن:
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

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

// حتما قبل از app.UseRouting()
app.UseForwardedHeaders();
app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Checklist}/{action=ArchiveIndex}/{id?}");

app.Run();
