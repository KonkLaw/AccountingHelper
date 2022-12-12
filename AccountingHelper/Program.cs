using AccountingHelper;
using AccountingHelper.Logic;
using AccountingHelper.ViewModels;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton<Storage>(new Storage());

builder.Services.AddScoped<ILoadFilesPageVM>(sp => new LoadFilesPageVM(sp, sp.GetService<IJSRuntime>()!));
builder.Services.AddScoped<ISelectionPageVM>(sp => new SelectionPageVM(sp));
builder.Services.AddScoped<ICategoriesPageVM>(sp => new CategoriesPageVM());
builder.Services.AddScoped<ISortingPageVM>(
    sp => new SortingPageVM(sp.GetService<Storage>()!));

await builder.Build().RunAsync();
