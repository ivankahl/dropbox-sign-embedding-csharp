using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DropboxSignEmbeddedSigning.Data;
using DropboxSignEmbeddedSigning.DropboxSign;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddScoped<DataSeeder>();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddRazorPages();

// Add Dropbox-related services that we need
var dropboxConfiguration = builder.Configuration.GetSection("DropboxSign").Get<DropboxSignConfiguration>() ??
                           throw new InvalidOperationException("Please provide a Dropbox Sign API Key and Client ID");
builder.Services.AddSingleton(dropboxConfiguration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

// Seed the database with an admin user
using var scope = app.Services.CreateScope();
await scope.ServiceProvider.GetRequiredService<DataSeeder>().SeedDataAsync();

app.Run();