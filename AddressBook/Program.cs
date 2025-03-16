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
using System.Reflection;
using System.Text;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ✅ Database Connection
var connectionString = builder.Configuration.GetConnectionString("SqlConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

// ✅ Register Services and Repositories
builder.Services.AddScoped<IAddressBookBL, AddressBookBL>();
builder.Services.AddScoped<IAddressBookRL, AddressBookRL>();
builder.Services.AddScoped<IUserBL, UserBL>();
builder.Services.AddScoped<IUserRL, UserRL>();
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<ICacheService, CacheService>();

// ✅ AutoMapper Profiles
builder.Services.AddAutoMapper(typeof(AddressBookProfile));
builder.Services.AddAutoMapper(typeof(UserMapper));

// ✅ RabbitMQ Services
builder.Services.AddSingleton<RabbitMqService>();
builder.Services.AddSingleton<RabbitMqConsumer>();
builder.Services.AddHostedService<RabbitMqBackgroundService>();

// ✅ Configure Redis
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"];
    options.InstanceName = builder.Configuration["Redis:InstanceName"];
});

// ✅ CORS Configuration
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

// ✅ Swagger Configuration
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Address Book API",
        Version = "v1",
        Description = "An API for managing address book contacts with authentication and RabbitMQ integration",
        Contact = new OpenApiContact
        {
            Name = "Aashutosh Kumar Pal",
            Email = "aashutoshkumarpal02@gmail.com"
        }
    });

    // ✅ Enable JWT Authentication in Swagger UI
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Enter 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // ✅ Add XML Comments for API Documentation
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

});


var app = builder.Build();

// ✅ Ensure Swagger is available in development & production (Optional)
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowAllOrigins");
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
