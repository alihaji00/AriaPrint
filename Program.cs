using AriaPrint.BL;
using AriaPrint.Repository;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Security.Claims;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
        options.SlidingExpiration = false;

        options.Events.OnValidatePrincipal = async context =>
        {
            var claimId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(claimId, out var userId))
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync();
                return;
            }

            var userRepo = context.HttpContext.RequestServices.GetRequiredService<UserRepository>();
            var user = await userRepo.GetByIdAsync(userId);

            var cookieRole = context.Principal!.FindFirst(ClaimTypes.Role)?.Value;
            if (user == null || user.Role != cookieRole)
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync();
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddSingleton<PasswordService>();
builder.Services.AddScoped<UserRepository>();

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    options.OnRejected = async (ctx, ct) =>
    {
        var http = ctx.HttpContext;

        var seconds = 60;
        if (ctx.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retry))
            seconds = (int)Math.Ceiling(retry.TotalSeconds);

        http.Response.StatusCode = 429;
        http.Response.Headers.RetryAfter = seconds.ToString();
        http.Response.ContentType = "text/html; charset=utf-8";

        var env = http.RequestServices.GetRequiredService<IWebHostEnvironment>();
        var html = await File.ReadAllTextAsync(Path.Combine(env.WebRootPath, "error-429.html"), ct);
        await http.Response.WriteAsync(html.Replace("{{SECONDS}}", seconds.ToString()), ct);
    };
    options.AddPolicy("Login", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1)
            }));
});

var app = builder.Build();

app.Use(async (context, next) =>
{
    var h = context.Response.Headers;
    h["X-Content-Type-Options"] = "nosniff";
    h["X-Frame-Options"] = "DENY";
    h["Referrer-Policy"] = "strict-origin-when-cross-origin";
    h["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    await next();
});

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

using (var scope = app.Services.CreateScope())
{
    var userRepo = scope.ServiceProvider.GetRequiredService<UserRepository>();
    var passwordService = scope.ServiceProvider.GetRequiredService<PasswordService>();

    long adminPhoneNumber = 09923365373;
    var existingAdmin = await userRepo.GetByPhoneNumberAsync(adminPhoneNumber);

    if (existingAdmin == null)
    {
        throw new Exception("ادمینی پیدا نشد!");
    }
}

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

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();