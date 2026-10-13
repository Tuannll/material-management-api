using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using TH4.Data;
using TH4.Middleware;
using TH4.Models.Common;
using TH4.Services.Implementations;
using TH4.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .Where(msg => !string.IsNullOrWhiteSpace(msg))
                .ToList();

            var response = ApiResponse<object>.FailureResponse("Dữ liệu đầu vào không hợp lệ.", errors, StatusCodes.Status400BadRequest);
            return new BadRequestObjectResult(response);
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TH4 - Quản Lý Vật Tư API",
        Version = "v1",
        Description = "ASP.NET Core Web API quản lý vật tư và nhà cung cấp, hỗ trợ so sánh LINQ Method Syntax và Query Syntax."
    });

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (!string.IsNullOrEmpty(connectionString))
    {
        options.UseSqlServer(connectionString);
    }
    else
    {
        options.UseInMemoryDatabase("TH4_Db");
    }
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<MethodSyntaxMaterialService>();
builder.Services.AddScoped<QuerySyntaxMaterialService>();

// Dynamic injection: chuyển đổi giữa Method Syntax và Query Syntax qua header 'X-Syntax-Style' hoặc query 'syntax=query|method'
builder.Services.AddScoped<IMaterialService>(sp =>
{
    var httpContext = sp.GetService<IHttpContextAccessor>()?.HttpContext;
    var syntax = httpContext?.Request.Headers["X-Syntax-Style"].FirstOrDefault()
                 ?? httpContext?.Request.Query["syntax"].FirstOrDefault();

    if (string.Equals(syntax, "query", StringComparison.OrdinalIgnoreCase))
    {
        return sp.GetRequiredService<QuerySyntaxMaterialService>();
    }

    return sp.GetRequiredService<MethodSyntaxMaterialService>();
});

var app = builder.Build();

// Ensure database and schema are created and seeded
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        dbContext.Database.EnsureCreated();
        DbInitializer.Initialize(dbContext);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Could not ensure database created directly; continuing.");
    }
}

app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TH4 - Quản Lý Vật Tư API v1");
    });
}

app.UseAuthorization();
app.MapControllers();
app.Run();
