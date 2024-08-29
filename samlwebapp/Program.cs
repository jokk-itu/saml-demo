using Saml2.Authentication.Core.Configuration;
using Saml2.Authentication.Core.Providers;
using samlwebapp;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.Configure<Saml2Configuration>(builder.Configuration.GetSection("Saml2"));
builder.Services.AddSaml();

// Override the registered services from AddSaml
builder.Services.AddTransient<ISamlXmlProvider, SamlXmlProvider>();

builder.Services.AddHttpContextAccessor();

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = "saml2.identity";
        options.DefaultChallengeScheme = "saml2";
    })
    .AddCookie("saml2.identity", options =>
    {
    })
    .AddSaml("saml2", options =>
    {
        options.SignInScheme = "saml2.identity";
        options.IdentityProviderName = "keycloak";

        options.DefaultRedirectUrl = "https://localhost:7006";
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
