using Crowd_Funding_Platform.Models;
using Crowd_Funding_Platform.Repositiories.Classes;
using Crowd_Funding_Platform.Repositiories.Classes.Authorization;
using Crowd_Funding_Platform.Repositiories.Classes.ManageCampaign;
using Crowd_Funding_Platform.Repositiories.Classes.UserProfile;
using Crowd_Funding_Platform.Repositiories.Interfaces;
using Crowd_Funding_Platform.Repositiories.Interfaces.IAuthorization;
using Crowd_Funding_Platform.Repositiories.Interfaces.IManageCampaign;
using Crowd_Funding_Platform.Repositiories.Interfaces.IUserProfile;
using Microsoft.AspNetCore.Authentication.Cookies;

using Microsoft.AspNetCore.Authentication.Google;

//using Crowd_Funding_Platform.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddSingleton<IEmailSenderRepos, EmailSenderRepos>(); // Email service interface and implementation


//// Register Google reCAPTCHA settings
//builder.Services.Configure<GoogleReCAPTCHA>(builder.Configuration.GetSection("GoogleReCAPTCHA"));

builder.Services.AddHttpClient();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie()
.AddGoogle(options =>
{
    options.ClientId = "1029890984360-j36jn12qf57suputfthmpm9fvmsd0hhi.apps.googleusercontent.com";
    options.ClientSecret = "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";
    options.CallbackPath = "/signin-google"; // or whatever you’ve set
});

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IAccountRepos, AccountClassRepos>();
builder.Services.AddScoped<ILoginRepos, LoginClassRepos>();
builder.Services.AddScoped<ICreatorApplicationRepos, CreatorApplicationRepos>();
builder.Services.AddScoped<ISidebarRepos, SidebarClassRepos>();
builder.Services.AddScoped<ICampaignsRepos, CampaignsClassRepos>();
builder.Services.AddScoped<IProfileRepos, ProfileClassRepos>();
builder.Services.AddScoped<IUser, UserClassRepos>();
builder.Services.AddScoped<ICategories, CategoriesClassRepos>();
builder.Services.AddScoped<IRewards, RewardsClassRepos>();
//builder.Services.AddScoped<IGoogleReCAPTCHAService, GoogleReCAPTCHAService>();


builder.Services.AddDbContext<DbMain_CFS>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout
    options.Cookie.HttpOnly = true; // Ensures session cookie is accessible only via HTTP
    options.Cookie.IsEssential = true; // Ensures cookie is essential
});

builder.Services.AddHttpContextAccessor();

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
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseSession();

//app.Use(async (context, next) =>
//{
//    context.Response.Headers.Add("Content-Security-Policy",
//        "default-src 'self'; script-src 'self' https://www.google.com https://www.gstatic.com; connect-src 'self' https://www.google.com;");
//    await next();
//});


app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
