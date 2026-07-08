using kutuphane.Repositories;
using kutuphane.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication(options =>
{
    // Varsayılan kimlik doğrulama şemalarını Cookie olarak kilitliyoruz 
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    // Çerezin adını tarayıcı net görsün 
    options.Cookie.Name = "SiberKutuphaneAuth";

    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);

    // Localhost ortamında çerezin tarayıcı tarafından reddedilmesini önlemek:
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax; // Sayfa geçişlerinde çerezi kaybettirmez
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // HTTPS yoksa bile localhost'ta çerezi engellemez kanka!

    options.SlidingExpiration = true;
});

var supabaseUrl = builder.Configuration["Supabase:Url"]
    ?? throw new InvalidOperationException("Supabase URL konfigürasyonu appsettings.json içinde bulunamadı!");

var supabaseKey = builder.Configuration["Supabase:Key"]
    ?? throw new InvalidOperationException("Supabase Key konfigürasyonu appsettings.json içinde bulunamadı!");
var supabaseOptions = new Supabase.SupabaseOptions
{
    AutoConnectRealtime = true
};
builder.Services.AddScoped(typeof(IRepository<>), typeof(SupabaseRepository<>));
builder.Services.AddScoped<BookService>();
builder.Services.AddScoped<MemberService>();
builder.Services.AddScoped<LoanService>();
builder.Services.AddScoped<BookLocationService>();
builder.Services.AddScoped<kutuphane.Services.IHomeService, kutuphane.Services.HomeService>();
builder.Services.AddSingleton(provider =>
    new Supabase.Client(supabaseUrl, supabaseKey, supabaseOptions));
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

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Welcome}/{id?}");

app.Run();
