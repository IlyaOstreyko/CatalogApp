using AutoMapper;
using CatalogApp.Infrastructure.Data;
using CatalogApp.Infrastructure.Interfaces;
using CatalogApp.Infrastructure.Mappings;
using CatalogApp.Infrastructure.Storage;
using CatalogApp.Infrastructure.UOW;
using CatalogApp.Server.Hubs;
using CatalogApp.Shared.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Text;
using IDbConnectionFactory = CatalogApp.Infrastructure.Interfaces.IDbConnectionFactory;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// MVC / OpenAPI
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// --- DB: connection string and factory
var connString = configuration.GetConnectionString("DefaultConnection")
    ?? throw new Exception("Connection string missing");

// Factory: singleton, lightweight, holds connection string and creates connections
builder.Services.AddSingleton<IDbConnectionFactory>(_ => new DbConnectionFactory(connString));

// Single IDbConnection per HTTP request (scoped)
builder.Services.AddScoped<IDbConnection>(sp =>
{
    var factory = sp.GetRequiredService<IDbConnectionFactory>();
    var conn = factory.CreateConnection();
    // Open here so that services can assume connection is open.
    if (conn.State != ConnectionState.Open)
        conn.Open();
    return conn;

});

// UnitOfWork: register concrete implementation as both interfaces
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUnitOfWorkWithTransaction>(sp => (IUnitOfWorkWithTransaction)sp.GetRequiredService<IUnitOfWork>());

// File storage: SqlFileStorage uses same connection and current transaction (if any)
builder.Services.AddScoped<IFileStorage>(sp =>
{
    var conn = sp.GetRequiredService<IDbConnection>();
    var uow = sp.GetService<IUnitOfWorkWithTransaction>();
    var tx = uow?.CurrentTransaction;
    return new SqlFileStorage(conn, tx);
});
// JWT
var jwt = configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();
if (string.IsNullOrWhiteSpace(jwt.Secret) || jwt.Secret.Length < 16)
{
    throw new Exception("JWT secret is missing or too short");
}
builder.Services.AddSingleton(jwt);
var key = Encoding.UTF8.GetBytes(jwt.Secret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = builder.Environment.IsDevelopment() ? false : true;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwt.Issuer,
        ValidateAudience = true,
        ValidAudience = jwt.Audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateLifetime = true
    };
});

builder.Services.AddAuthorization();

// CORS: разрешаем клиенту подключаться (в development ограничьте origin)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClient", policy =>
    {
        policy.WithOrigins("https://localhost:7088", "http://localhost:5145") // укажите URL клиента
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddSignalR();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowClient");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<CatalogHub>("/hubs/catalog");

app.Run();
