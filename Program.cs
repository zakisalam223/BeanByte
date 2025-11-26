// Program.cs
using forum_aspcore.Models;
using forum_aspcore.Services;
using forum_aspcore.Stores;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Razor;


var builder = WebApplication.CreateBuilder(args);

// loading environment variables
DotNetEnv.Env.Load();

builder.Configuration["MongoDBSettings:ConnectionURI"] =
    Environment.GetEnvironmentVariable("MONGOSTRING");

builder.Services.Configure<MongoDBSettings>(
    builder.Configuration.GetSection("MongoDBSettings"));

// Register DatabaseService as a singleton since it manages the database connection
builder.Services.AddSingleton<DatabaseService>();

// Register Store classes
builder.Services.AddScoped<MongoUserStore>();
builder.Services.AddScoped<MongoThreadStore>();
builder.Services.AddScoped<MongoPrivateMessageStore>();
builder.Services.AddScoped<MongoFileStore>();
builder.Services.AddScoped<MongoRepLogStore>();
builder.Services.AddScoped<MongoTagStore>();
builder.Services.AddScoped<MongoInfractionStore>();
builder.Services.AddScoped<MongoSectionStore>();

// Add authentication services
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Users/Login"; // Redirect to login page if not authenticated
        options.LogoutPath = "/Users/Logout"; // Redirect to logout page
        options.Cookie.Name = "UserAuthCookie"; // Set a name for the authentication cookie
        options.ExpireTimeSpan = TimeSpan.FromDays(5); // Set cookie expiration to 5 days
        options.SlidingExpiration = true; // Renew cookie expiration on activity
        options.Cookie.HttpOnly = true; // Protect the cookie from JavaScript access
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Use Secure cookies if served over HTTPS
    });



if (builder.Environment.IsDevelopment())
{
    builder.Services.AddControllersWithViews()
        .AddRazorRuntimeCompilation();
}
else
{
    builder.Services.AddControllersWithViews();
}

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStatusCodePagesWithReExecute("/Error/{0}");
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
   // pattern: "{controller=Home}/{action=Index}/{id?}");
   pattern: "{controller=Section}/{action=Index}/{id?}");

app.Run();
