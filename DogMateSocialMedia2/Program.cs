using MySql.Data.MySqlClient;
using System.Data;
using DapperProj.Repositories;
using DapperProj.Repositories.Interfaces;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "DogMateSocialMedia API",
        Description = "An ASP.NET Core Web API for managing users and comments",
    });
});

builder.Configuration.SetBasePath(Directory.GetCurrentDirectory());
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

// Реєструємо MySqlConnection
builder.Services.AddScoped<MySqlConnection>(_ =>
    new MySqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));

// Реєструємо IDbTransaction
builder.Services.AddScoped<IDbTransaction>(sp =>
{
    var connection = sp.GetRequiredService<MySqlConnection>();
    connection.Open(); // Відкриваємо підключення
    return connection.BeginTransaction(); // Починаємо транзакцію
});

// Реєструємо репозиторії та UnitOfWork
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();