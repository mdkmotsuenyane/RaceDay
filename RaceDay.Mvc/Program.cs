using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RaceDay.Mvc.Services;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add MVC
builder.Services.AddControllersWithViews();

// Add Session Support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Register the API Client (talks to RaceDay.Api)
builder.Services.AddHttpClient<ApiClient>(client =>
{
    // 1. Look for Docker config first, otherwise fallback to local HTTP
    var apiUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5100/";
    client.BaseAddress = new Uri(apiUrl);
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    // 2. This tells the HttpClient to bypass SSL certificate errors locally
    var handler = new HttpClientHandler();
    handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
    return handler;
});

// Add HttpContextAccessor so ApiClient can read the session
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// We commented this out so it doesn't force HTTPS and crash in Docker or local HTTP
// app.UseHttpsRedirection(); 

app.UseStaticFiles();
app.UseRouting();

// Enable Session Middleware (must be before UseAuthorization)
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
