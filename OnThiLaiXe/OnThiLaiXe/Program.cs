using Hangfire;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.DotNet.Scaffolding.Shared;
using Microsoft.EntityFrameworkCore;
using OnThiLaiXe.Models;
using OnThiLaiXe.Repositories;
using OnThiLaiXe.Services;
using Microsoft.AspNetCore.StaticFiles;
using OfficeOpenXml;
using OnThiLaiXe.Middleware;
using sendMail.Service;
var builder = WebApplication.CreateBuilder(args);
ExcelPackage.License.SetNonCommercialPersonal("Webonlaixe");
// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
        .AddDefaultTokenProviders()
        .AddDefaultUI()
        .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddRazorPages();
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});
builder.Services.AddAuthorization();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
        options.SignInScheme = IdentityConstants.ExternalScheme; //de gg hd chung voi identity
    });
builder.Services.AddMemoryCache();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IBaiThiRepository, BaiThiRepository>();
builder.Services.AddScoped<ICauHoiRepository, EFCauHoiRepository>();
builder.Services.AddScoped<IChuDeRepository, EFChuDeRepository>();
builder.Services.AddScoped<ILoaiBangLaiRepository, EFLoaiBangLaiRepository>();
builder.Services.AddTransient<IGmailSender, GmailSender>();
builder.Services.AddScoped<IUserRepository, EFUserRepository>();
builder.Services.AddScoped<IBaiSaHinhRepository, EFBaiSaHinhRepository>();
builder.Services.AddScoped<IGiaoDichRepository, EFGiaoDichRepository>();
builder.Services.AddScoped<IVisitLogRepository, EFVisitLogRepository>();
builder.Services.AddScoped<IMoPhongRepository, MoPhongRepository>();
builder.Services.AddHangfire(configuration => configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"),
        new Hangfire.SqlServer.SqlServerStorageOptions
        {
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            QueuePollInterval = TimeSpan.Zero,
            UseRecommendedIsolationLevel = true,
            UsePageLocksOnDequeue = true,
            DisableGlobalLocks = true
        }));
builder.Services.AddHangfireServer();
builder.Services.AddSignalR();
var app = builder.Build();
//app.MapHub<OnlineUsersHub>("/onlineUsersHub");
app.UseSession();
app.UseStaticFiles();
app.UseMiddleware<VisitMiddleware>();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.UseHangfireDashboard();
app.UseEndpoints(endpoints =>
{
    // Route dành cho khu vực Admin
    endpoints.MapControllerRoute(
        name: "Admin",
        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
    );

    // Route mặc định
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}"
    );

    // Kích hoạt Razor Pages
    endpoints.MapRazorPages();
});

// Xử lý static assets
app.MapStaticAssets();

app.Run();