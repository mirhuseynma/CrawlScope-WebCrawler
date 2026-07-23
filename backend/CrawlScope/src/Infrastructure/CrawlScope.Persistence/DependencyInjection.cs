using CrawlScope.Persistence.Interceptors;

namespace CrawlScope.Persistence
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<DiagnosticDbCommandInterceptor>();

            services.AddDbContext<AppDbContext>((sp, options) =>
            {
                var interceptor = sp.GetRequiredService<DiagnosticDbCommandInterceptor>();
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
                options.AddInterceptors(interceptor);
                options.EnableSensitiveDataLogging(); // useful for diagnosis
            });

            services
                .AddIdentity<AppUser, IdentityRole>(options =>
                {
                    options.Password.RequiredLength = 8;
                    options.Password.RequireDigit = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireNonAlphanumeric = true;
                    options.User.RequireUniqueEmail = true;
                    options.SignIn.RequireConfirmedEmail = true;
                })
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());
            services.AddScoped<IJwtService, JwtService>();

            return services;
        }
    }
}
