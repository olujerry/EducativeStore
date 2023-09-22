using Ecommerce.Application.Common;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Identity.Entities;
using Ecommerce.Domain.Identity.Permissions;
using Ecommerce.Domain.Models;
using Ecommerce.Infrastructure.Sql.DataAccess;
using Ecommerce.Infrastructure.Sql.Identity;
using Ecommerce.Infrastructure.Sql.Identity.Filters;
using Ecommerce.Infrastructure.Sql.Identity.Permission;
using Ecommerce.Infrastructure.Sql.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Ecommerce.Infrastructure.Sql.Ioc;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureSqlServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<DataContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("AppConnection"), 
                    builder => builder.MigrationsAssembly(typeof(DataContext).Assembly.FullName)), ServiceLifetime.Transient);

        //services.AddDbContext<DataContext>(options =>
        //    options.UseNpgsql(configuration.GetConnectionString("PostgreSql"),
        //        builder => builder.MigrationsAssembly(typeof(DataContext).Assembly.FullName)), ServiceLifetime.Transient);

        services.AddScoped<IDataContext>(provider => provider.GetRequiredService<DataContext>());


        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        var k = services.BuildServiceProvider().GetService<IKeyAccessor>();
        var conSec = (k != null && k?["SecurityConfiguration"] != null) ? JsonSerializer.Deserialize<SecurityConfiguration>(k["SecurityConfiguration"]) : new SecurityConfiguration();
        var conAdv = (k != null && k?["AdvancedConfiguration"] != null) ? JsonSerializer.Deserialize<AdvancedConfiguration>(k["AdvancedConfiguration"]) : new AdvancedConfiguration();


        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Password settings.
                options.Password.RequireDigit = conSec?.IsPasswordRequireDigit ?? false;
                options.Password.RequireLowercase = conSec?.IsPasswordRequireLowercase ?? false;
                options.Password.RequireUppercase = conSec?.IsPasswordRequireUppercase ?? false;
                options.Password.RequireNonAlphanumeric = conSec?.IsPasswordRequireNonAlphanumeric ?? false;
                options.Password.RequiredLength = conSec?.PasswordRequiredLength ?? 4;
                options.Password.RequiredUniqueChars = 1;
                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(conSec?.UserLockoutTime ?? 0);
                options.Lockout.MaxFailedAccessAttempts = conSec?.MaxFailedAccessAttempts ?? 9999;
                options.Lockout.AllowedForNewUsers = true;
                // User settings.
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = conAdv?.EnableEmailConfirmation ?? false;
            })
            .AddEntityFrameworkStores<DataContext>()
            .AddClaimsPrincipalFactory<CustomUserClaimsPrincipalFactory>()
            .AddDefaultTokenProviders();


        services.AddSingleton<IPermissionHelper, PermissionHelper>();

        services.Configure<SecurityStampValidatorOptions>(options =>
        {
            options.ValidationInterval = TimeSpan.Zero;
        });

        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddTransient<IEmailManager, EmailManager>();

        services.AddTransient<IApplicationUserManager, ApplicationUserManager>();
        services.AddTransient<IApplicationRoleManager, ApplicationRoleManager>();
        services.AddTransient<IApplicationSignInManager, ApplicationSignInManager>();
        services.AddTransient<IApplicationProfileManager, ApplicationProfileManager>();

        return services;
    }


    //public static void ExecuteMigrate(IServiceProvider services)
    //{
    //    using (var scope = services.CreateScope())
    //    {
    //        var dbContext = scope.ServiceProvider.GetService<DataContext>();
    //        dbContext.Database.Migrate();
    //        //SeedData(dbContext);
    //    }
    //}

    //private static void SeedData(DataContext dbContext)
    //{
    //    dbContext.Database.ExecuteSqlRaw("EXECUTE dbo.MyStoredProcedure");
    //    // add more seeding code here if needed
    //}

}
