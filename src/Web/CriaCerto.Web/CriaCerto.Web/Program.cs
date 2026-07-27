using Microsoft.AspNetCore.Authentication.Cookies;
using CriaCerto.Web.Components;
using CriaCerto.Web.Client.Services;
using CriaCerto.Web.Client.Auth;

using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// Configure Data Protection to persist keys across container restarts
var keysDirectory = new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "dataprotection-keys"));
if (!keysDirectory.Exists)
{
    keysDirectory.Create();
}

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(keysDirectory)
    .SetApplicationName("CriaCertoWeb");

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>(sp => 
    sp.GetRequiredService<CustomAuthStateProvider>());
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
    });
builder.Services.AddAuthorization();
builder.Services.AddScoped(sp => new HttpClient());
builder.Services.AddScoped<PlantelApiClient>();
builder.Services.AddScoped<BreedingOpsApiClient>();
builder.Services.AddScoped<GrowthApiClient>();
builder.Services.AddScoped<NutritionApiClient>();
builder.Services.AddScoped<TenancyApiClient>();
builder.Services.AddScoped<IOfflineSyncService, OfflineSyncService>();
builder.Services.AddScoped<IToastService, ToastService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Microsoft.AspNetCore.Antiforgery.AntiforgeryValidationException)
    {
        foreach (var cookieKey in context.Request.Cookies.Keys)
        {
            if (cookieKey.StartsWith(".AspNetCore.Antiforgery", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.Cookies.Delete(cookieKey);
            }
        }

        if (!context.Response.HasStarted)
        {
            context.Response.Redirect(context.Request.Path);
        }
    }
});

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(CriaCerto.Web.Client._Imports).Assembly);

app.Run();
