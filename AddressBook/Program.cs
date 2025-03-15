using BusinessLayer.Interface;
using BusinessLayer.Mapping;
using BusinessLayer.Service;
using BusinessLayer.Validator;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using StackExchange.Redis;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Middleware.Authenticator;
using Middleware.Email;
using Middleware.RabbitMQ;
using ModelLayer.Model;
using RepositoryLayer.Context;
using RepositoryLayer.Interface;
using RepositoryLayer.Service;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ✅ Database Connection
var connectionString = builder.Configuration.GetConnectionString("SqlConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

// ✅ Register Services and Repositories
builder.Services.AddScoped<IAddressBookBL, AddressBookBL>();
builder.Services.AddScoped<IAddressBookRL, AddressBookRL>();

builder.Services.AddAutoMapper(typeof(AddressBookProfile));
builder.Services.AddAutoMapper(typeof(UserMapper));

builder.Services.AddScoped<IUserBL, UserBL>();
builder.Services.AddScoped<IUserRL, UserRL>();
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<ICacheService, CacheService>(); // ✅ Use Custom Cache Service

// ✅ Register RabbitMQ
builder.Services.AddSingleton<RabbitMqService>();
builder.Services.AddSingleton<RabbitMqConsumer>(); // ✅ Ensure Consumer is Registered
builder.Services.AddHostedService<RabbitMqBackgroundService>(); // ✅ Run Consumer as Background Service

// ✅ Configure Redis
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"];
    options.InstanceName = builder.Configuration["Redis:InstanceName"];
});

// ✅ Configure CORS (Fix: Allow Frontend Integration)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

// ✅ Session Management
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ✅ Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSettings["Key"];

if (string.IsNullOrEmpty(jwtKey))
{
    throw new Exception("JWT Key is missing in configuration");
}

var key = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// ✅ FluentValidation Configuration
builder.Services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters();
builder.Services.AddScoped<IValidator<RequestAddressBook>, RequestAddressBookValidator>();

// ✅ Add Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ✅ Ensure Swagger is available in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("AllowAllOrigins"); // ✅ Fix: Enable CORS

app.UseSession(); // ✅ Fix: Move session middleware before authentication

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
