using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using EduMentorClient.Services;
using Blazored.LocalStorage;
using Client; 

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5000/") });

builder.Services.AddMudServices();
builder.Services.AddSingleton<UserState>();
builder.Services.AddBlazoredLocalStorage();

await builder.Build().RunAsync();