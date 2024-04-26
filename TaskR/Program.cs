using Microsoft.AspNetCore.Authentication.Cookies;
using TaskR.Data;
using TaskR.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<TaskRContext>();
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<CryptoService256>();


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opts =>
    {
        //Der LoginPath ist per Default auf "/Account/Login", kann aber frei ge�ndert werden
        opts.LoginPath = "/Auth/Login";


        opts.AccessDeniedPath = "/Home/Index";
    });

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
