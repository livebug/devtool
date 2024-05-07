using Microsoft.AspNetCore.Components;
using AntDesign.ProLayout;
using Microsoft.EntityFrameworkCore;
using Config;

var builder = WebApplication.CreateBuilder(args);

// 增加配置数据库支持
string? config_str = builder.Configuration.GetConnectionString("ConfigContext"); 
builder.Configuration.AddDbConfig(
    opt => opt.UseNpgsql(config_str)
);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddAntDesign();
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(sp.GetService<NavigationManager>()!.BaseUri)
});
builder.Services.Configure<ProSettings>(builder.Configuration.GetSection("ProSettings"));
builder.Services.AddDbContextFactory<Bloging.BlogingDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("BloggingContext")));
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

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();