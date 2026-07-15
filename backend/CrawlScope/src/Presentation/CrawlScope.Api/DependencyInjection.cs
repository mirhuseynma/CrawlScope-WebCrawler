namespace CrawlScope.Api;

public static class DependencyInjection
{
    public const string DashboardCorsPolicy = "DashboardCorsPolicy";

    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(entry => entry.Value?.Errors.Count > 0)
                    .ToDictionary(
                        entry => entry.Key,
                        entry => entry.Value!.Errors.Select(error => error.ErrorMessage).ToArray());

                var response = ProblemDetailsFactory.Create(
                    context.HttpContext,
                    StatusCodes.Status400BadRequest,
                    "Validation failed.",
                    errors);

                return new BadRequestObjectResult(response);
            };
        });

        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        services.AddCors(options =>
        {
            options.AddPolicy(DashboardCorsPolicy, policy =>
            {
                policy
                    .WithOrigins("http://localhost:5173")
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
            {
                Title = "CrawlScope.Api",
                Version = "v1"
            });
            options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.OpenApiSecurityScheme
            {
                In = Microsoft.OpenApi.ParameterLocation.Header,
                Description = "Paste only the JWT token. Swagger will send it as a Bearer token.",
                Name = "Authorization",
                Type = Microsoft.OpenApi.SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "bearer"
            });
            options.AddSecurityRequirement(document => new Microsoft.OpenApi.OpenApiSecurityRequirement
            {
                [new Microsoft.OpenApi.OpenApiSecuritySchemeReference("Bearer", document, null)] = []
            });
        });

        return services;
    }
}
