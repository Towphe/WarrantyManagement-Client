using Microsoft.EntityFrameworkCore;
using src.Model.Repo;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDbContext<WarrantyrepoContext>(opts =>{
    opts.UseNpgsql(builder.Configuration.GetConnectionString("db_key"));
});
builder.Host.ConfigureLogging(logging =>{
    logging.ClearProviders();
    logging.AddConsole();
});
builder.Services.AddControllersWithViews().AddSessionStateTempDataProvider();
builder.Services.AddSession();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error"); // alter later
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.UseSession();
app.MapDefaultControllerRoute();

app.Run();
