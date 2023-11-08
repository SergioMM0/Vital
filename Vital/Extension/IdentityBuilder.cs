﻿using Microsoft.AspNetCore.Identity;
using Vital.Data;
using Vital.Models.Identity;

namespace Vital.Extension;

public static class IdentityBuilder {
    public static IServiceCollection SetupIdentity(this IServiceCollection services) {
        services.AddIdentityCore<ApplicationUser>(options => {
            // Password settings.
            options.Password.RequireDigit = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequiredLength = 6;
        })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        return services;
    }
}
