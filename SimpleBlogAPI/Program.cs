using Microsoft.EntityFrameworkCore;
using SimpleBlogAPI.Models;
using Serilog;
using Microsoft.AspNetCore.Mvc;
using SimpleBlogAPI.Middleware;
using Microsoft.OpenApi.Models;
using SimpleBlogAPI.Filters.ActionFilters;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
});

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

var _connectionString = builder.Configuration.GetConnectionString("BlogConnection");

builder.Services.AddDbContext<BlogContext>(
        options => options
            .UseMySql(_connectionString, ServerVersion.AutoDetect(_connectionString))
            .EnableDetailedErrors()
);

builder.Services.AddAutoMapper(typeof(Program));

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddScoped<ValidationFilterAttribute>();
builder.Services.AddScoped<ValidatePostExistsAttribute>();
builder.Services.AddScoped<ValidateCommentForPostExistsAttribute>();
builder.Services.AddScoped<ValidateCategoryExistsAttribute>();

builder.Services.AddSwaggerGen(s =>
{
    s.SwaggerDoc("v1", new OpenApiInfo { Title = "Simple Blog API", Version = "v1" });
});

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseSwagger();
app.UseSwaggerUI(s =>
{
    s.SwaggerEndpoint("/swagger/v1/swagger.json", "Simple Blog API v1");
});

app.UseAuthorization();

app.MapControllers();

app.Run();