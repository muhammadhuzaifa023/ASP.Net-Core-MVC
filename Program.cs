using ASP.Net_Core_MVC.Data;
using ASP.Net_Core_MVC.Infrastructure.Generic;
using ASP.Net_Core_MVC.Infrastructure.IGeneric;
using ASP.Net_Core_MVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(connectionString));
// //Configuration Identity Services
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(
    options =>
    {
        // Password settings
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequiredUniqueChars = 4;


        // Rest of the Code

        // Lockout settings
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30); // Lockout duration
        options.Lockout.MaxFailedAccessAttempts = 5; // Number of failed attempts allowed
        options.Lockout.AllowedForNewUsers = true; // Lockout new users


        // Other settings can be configured here
    }).AddEntityFrameworkStores<DataContext>().AddDefaultTokenProviders();
builder.Services.AddAutoMapper(typeof(Program).Assembly);
// Configure the Application Cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    // If the LoginPath isn't set, ASP.NET Core defaults the path to /Account/Login.
    options.LoginPath = "/Account/Login"; // Set your login path here
                                          // If the AccessDenied isn't set, ASP.NET Core defaults the path to /Account/AccessDenied
    options.AccessDeniedPath = "/Account/AccessDenied";
});
// Configure token lifespan
builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    // Set token lifespan to 2 hours
    options.TokenLifespan = TimeSpan.FromHours(2);
});
builder.Services.AddScoped<UserManager<ApplicationUser>>();
builder.Services.AddScoped<SignInManager<ApplicationUser>>();
builder.Services.AddScoped<RoleManager<ApplicationRole>>();
builder.Services.AddScoped<ISenderEmail, EmailSender>();

builder.Services.AddScoped<IProduct, ProductService>();


// -------------------------------------  Claimed Based Authorization with Policy claims ---------------------------------------------//
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("EditRolePolicy", policy => policy.RequireClaim("Edit Role"));
    options.AddPolicy("DeleteRolePolicy", policy => policy.RequireClaim("Delete Role"));
});
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseStatusCodePagesWithReExecute("/Error/{0}");
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
