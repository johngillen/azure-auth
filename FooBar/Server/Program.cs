using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add configuration to the container.

builder.Configuration.AddJsonFile($"appsettings.json", optional: true, reloadOnChange: true);
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// JWT Authentication
var openidconfig = new ConfigurationManager<OpenIdConnectConfiguration>(
  $"{builder.Configuration["AzureAD:Instance"]}{builder.Configuration["AzureAD:TenantID"]}/v2.0/.well-known/openid-configuration", 
  new OpenIdConnectConfigurationRetriever());
var openidkeys = openidconfig.GetConfigurationAsync().Result.SigningKeys;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
  .AddJwtBearer(options => {
    builder.Configuration.Bind("AzureAd", options);
    options.TokenValidationParameters.IssuerSigningKeys = openidkeys;
    options.TokenValidationParameters.ValidIssuers = new[] {
      $"{builder.Configuration["AzureAD:Instance"]}{builder.Configuration["AzureAD:TenantID"]}/v2.0",
      "https://login.microsoftonline.com/dddddddd-dddd-dddd-dddd-dddddddddddddddd/v2.0"
      // Add more issuers for a multi-tenant app
    };
});

builder.Services.AddAuthorization(options => {
  options.AddPolicy("Bearer", new AuthorizationPolicyBuilder()
  .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
  .RequireAuthenticatedUser().Build());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseWebAssemblyDebugging();
}
else
{
  app.UseExceptionHandler("/Error");
  // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
  app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
