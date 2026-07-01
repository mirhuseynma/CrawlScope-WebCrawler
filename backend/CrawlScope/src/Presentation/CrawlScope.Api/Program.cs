using CrawlScope.Api.Common.Middleware;
using CrawlScope.Api.Common.Http;
using CrawlScope.Application;
using CrawlScope.Infrastructure;
using CrawlScope.Persistence;
using Microsoft.OpenApi;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
const string dashboardCorsPolicy = "DashboardCorsPolicy";

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddApplication();
builder.Services.AddInfrastructure();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddCors(options =>
{
    options.AddPolicy(dashboardCorsPolicy, policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CrawlScope.Api",
        Version = "v1"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseStatusCodePages(async statusCodeContext =>
{
    var httpContext = statusCodeContext.HttpContext;

    if (httpContext.Response.HasStarted || httpContext.Response.StatusCode < StatusCodes.Status400BadRequest)
    {
        return;
    }

    var problemDetails = ProblemDetailsFactory.Create(
        httpContext,
        httpContext.Response.StatusCode);

    await httpContext.Response.WriteAsJsonAsync(problemDetails);
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.RoutePrefix = "swagger";
        options.SwaggerEndpoint("v1/swagger.json", "CrawlScope.Api v1");
    });
}

app.UseHttpsRedirection();

app.UseCors(dashboardCorsPolicy);

app.UseAuthorization();

app.MapControllers();

app.Run();
