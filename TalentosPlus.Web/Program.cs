using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TalentosPlus.Application.Services;
using TalentosPlus.Application.Interfaces;
using TalentosPlus.Domain.Interfaces;
using TalentosPlus.Infrastructure.Persistence;
using TalentosPlus.Infrastructure.Repositories;
using TalentosPlus.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false) // Relaxed for admin demo
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAiService, GeminiAiService>();
builder.Services.AddScoped<IPdfService, PdfService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped(sp => new HttpClient());

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// Auto-migration on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try 
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
        // Seed Admin User
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        if (!userManager.Users.Any())
        {
            var admin = new IdentityUser { UserName = "admin@talentosplus.com", Email = "admin@talentosplus.com", EmailConfirmed = true };
            userManager.CreateAsync(admin, "Admin123!").Wait();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}

app.Run();
