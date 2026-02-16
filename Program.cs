using DesertPaths.Configuration;
using DesertPaths.Constants;
using DesertPaths.Data;
using DesertPaths.Models.Entities;
using DesertPaths.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DesertPaths
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            // Use PostgreSQL in production (Koyeb), SQL Server in development
            var databaseProvider = builder.Configuration["DatabaseProvider"] ?? "SqlServer";

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                if (databaseProvider == "PostgreSQL" || builder.Environment.IsProduction())
                {
                    options.UseNpgsql(connectionString);
                }
                else
                {
                    options.UseSqlServer(connectionString);
                }
            });
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => 
            {
                // TODO: Set to true when email service is configured
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders()
            .AddDefaultUI();

            // External Authentication (Google & Microsoft)
            var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
            var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
            var microsoftClientId = builder.Configuration["Authentication:Microsoft:ClientId"];
            var microsoftClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"];

            var authBuilder = builder.Services.AddAuthentication();

            // Add Google Authentication if configured
            if (!string.IsNullOrEmpty(googleClientId) && 
                !googleClientId.StartsWith("YOUR_") &&
                !string.IsNullOrEmpty(googleClientSecret))
            {
                authBuilder.AddGoogle(options =>
                {
                    options.ClientId = googleClientId;
                    options.ClientSecret = googleClientSecret;
                    options.Scope.Add("profile");
                    options.Scope.Add("email");
                });
            }

            // Add Microsoft Authentication if configured
            if (!string.IsNullOrEmpty(microsoftClientId) && 
                !microsoftClientId.StartsWith("YOUR_") &&
                !string.IsNullOrEmpty(microsoftClientSecret))
            {
                authBuilder.AddMicrosoftAccount(options =>
                {
                    options.ClientId = microsoftClientId;
                    options.ClientSecret = microsoftClientSecret;
                });
            }

            // Application Services
            builder.Services.AddScoped<IBookingService, BookingService>();

            // Payment Service Configuration
            // Use Mock for development, PayTabs for production
            builder.Services.Configure<Configuration.PayTabsSettings>(
                builder.Configuration.GetSection(Configuration.PayTabsSettings.SectionName));

            var payTabsProfileId = builder.Configuration["PayTabs:ProfileId"];
            var useMockPayment = string.IsNullOrEmpty(payTabsProfileId) || 
                                 payTabsProfileId.StartsWith("YOUR_") ||
                                 builder.Environment.IsDevelopment();

            if (useMockPayment)
            {
                // Use Mock Payment Service for development/testing
                builder.Services.AddScoped<IPaymentService, MockPaymentService>();
            }
            else
            {
                // Use PayTabs for production
                builder.Services.AddHttpClient<IPaymentService, PayTabsPaymentService>();
            }

            // Authorization Policies
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAdmin", policy =>
                    policy.RequireRole(AppRoles.Admin));

                options.AddPolicy("RequireManager", policy =>
                    policy.RequireRole(AppRoles.Admin, AppRoles.Manager));

                options.AddPolicy("RequireCustomer", policy =>
                    policy.RequireRole(AppRoles.Admin, AppRoles.Manager, AppRoles.Customer));

                options.AddPolicy("CanManageContent", policy =>
                    policy.RequireRole(AppRoles.Admin, AppRoles.Manager));

                options.AddPolicy("CanDeleteContent", policy =>
                    policy.RequireRole(AppRoles.Admin));

                options.AddPolicy("CanManageUsers", policy =>
                    policy.RequireRole(AppRoles.Admin));

                options.AddPolicy("CanBlockUsers", policy =>
                    policy.RequireRole(AppRoles.Admin, AppRoles.Manager));
            });

            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Create/Migrate database automatically
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // For PostgreSQL in production, ensure database is created
                if (app.Environment.IsProduction())
                {
                    await dbContext.Database.EnsureCreatedAsync();
                }
                else
                {
                    await dbContext.Database.MigrateAsync();
                }
            }

            // Seed data
            await DataSeeder.SeedAllAsync(app.Services, builder.Configuration);

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
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();
            app.MapRazorPages()
               .WithStaticAssets();

            app.Run();
        }
    }
}
