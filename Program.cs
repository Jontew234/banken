using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Platinum.Areas.Identity.Data;
using Platinum.Controllers;
using DotNetEnv;


var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("MinDbContextConnection") ?? throw new InvalidOperationException("Connection string 'MinDbContextConnection' not found.");

builder.Services.AddDbContext<MinDbContext>(options =>
    options.UseSqlServer(connectionString).LogTo(Console.WriteLine, LogLevel.Information));

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAuthentication("MyCookieAuth").AddCookie("MyCookieAuth", options =>
{
    options.Cookie.Name = "MyCookieAuth";
    options.LoginPath = "Account/Login";
    options.AccessDeniedPath = "/Account/Login";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);

});
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
});




builder.Services.AddDefaultIdentity<Customer>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<MinDbContext>();


builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();


builder.Services.AddSingleton<AssetApiController>();
builder.Services.AddSingleton<Events>();
builder.Services.AddSingleton<MyTimerHostedService>();
builder.Services.AddSingleton<IHostedService, MyTimerHostedService>();
builder.Services.AddScoped<InvestmentController>();
builder.Services.AddScoped<CustomerController>();
builder.Services.AddScoped<TransactionController>();
builder.Services.AddScoped<InvoiceController>();

builder.Services.AddSingleton<RentTimerEvent>();
builder.Services.AddSingleton<IHostedService, RentTimerEvent>();














// hämta nyckeln
DotNetEnv.Env.Load();
var FinanceKey = Environment.GetEnvironmentVariable("Finance");
var CryptoKey = Environment.GetEnvironmentVariable("Crypto");

builder.Services.AddHttpClient("Finance", client =>
{
    client.BaseAddress = new Uri("https://real-time-finance-data.p.rapidapi.com/");
    client.DefaultRequestHeaders.Add("X-RapidAPI-Key", FinanceKey);
    client.DefaultRequestHeaders.Add("X-RapidAPI-Host", "real-time-finance-data.p.rapidapi.com");
});

// Ändra baseadress sedan är den korrekt
builder.Services.AddHttpClient("Crypto", client =>
{
    client.BaseAddress = new Uri("https://coinranking1.p.rapidapi.com");
    client.DefaultRequestHeaders.Add("X-RapidAPI-Key", CryptoKey);
    client.DefaultRequestHeaders.Add("X-RapidAPI-Host", "coinranking1.p.rapidapi.com");
});


// Add services to the container.
builder.Services.AddControllersWithViews().AddNewtonsoftJson();

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
app.UseSession();
app.UseRouting();
app.UseAuthentication(); ;

app.UseAuthorization();





//app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
