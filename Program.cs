using System.Globalization;
using Aimidge.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { new CultureInfo("en"), new CultureInfo("lt") };
    options.DefaultRequestCulture = new RequestCulture("en");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

builder.Services.AddMvc().AddViewLocalization().AddDataAnnotationsLocalization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CookieService>();
builder.Services.AddScoped<CryptoService>();
builder.Services.AddScoped<DatabaseService>();
builder.Services.AddScoped<SDService>();
builder.Services.AddScoped<TranslationService>();
builder.Services.AddScoped<ValidationService>();
builder.Services.AddHttpClient();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var localizationOptions = services
        .GetRequiredService<IOptions<RequestLocalizationOptions>>()
        .Value;
    app.UseRequestLocalization(localizationOptions);
}

app.UseAuthorization();

app.MapRazorPages();

app.MapControllers();

app.Run();
