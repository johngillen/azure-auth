using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using FooBar.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddHttpClient("FooBar.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
  .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("FooBar.ServerAPI"))
  .AddLogging(options => {
    options.SetMinimumLevel(LogLevel.Trace);
    options.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);;
});

builder.Services.AddMsalAuthentication(options => {
  builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
  options.ProviderOptions.DefaultAccessTokenScopes.Add(
    builder.Configuration.GetSection("ServerApi")["Scopes"] ?? throw new InvalidOperationException("ServerApi:Scopes is missing from appsettings.json")
  );
});

await builder.Build().RunAsync();
