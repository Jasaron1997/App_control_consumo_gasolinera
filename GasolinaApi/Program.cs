using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;
using GasolinaApi.Auth;
using GasolinaApi.Data;
using GasolinaApi.DTOs;
using GasolinaApi.Filters;
using GasolinaApi.Middleware;
using GasolinaApi.Services;
using GasolinaApi.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ---- Configuración ----
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
    ?? throw new InvalidOperationException("Falta la sección 'Jwt' en la configuración.");

// appsettings.json ya trae la sección 'Jwt' presente (con Secret vacío como placeholder),
// así que el throw de arriba nunca dispara por un Secret faltante: se valida aparte,
// para fallar rápido con un mensaje claro en vez de una excepción críptica de
// Microsoft.IdentityModel (IDX10703) en la primera solicitud autenticada.
if (string.IsNullOrWhiteSpace(jwtSettings.Secret))
{
    throw new InvalidOperationException(
        "Falta configurar 'Jwt:Secret'. En local, usa: dotnet user-secrets set \"Jwt:Secret\" \"<clave>\".");
}

var origenesPermitidos = builder.Configuration.GetSection("Cors:OrigenesPermitidos").Get<string[]>() ?? [];

// Mismas opciones de serialización que ManejoErroresMiddleware, para que el 429
// del rate limiter (que no pasa por ese middleware) use el mismo camelCase.
var opcionesJsonRespuestaApi = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

// ---- Base de datos (Database First: el esquema ya existe, no se generan migraciones) ----
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("GasolinaDb")));

// ---- Autenticación JWT ----
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
        ClockSkew = TimeSpan.FromMinutes(1)
    };
});

builder.Services.AddAuthorization();

// ---- Rate limiting (login: 5 intentos/minuto por IP, contra fuerza bruta) ----
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Mismo sobre RespuestaApi<T> que ManejoErroresMiddleware: el rechazo del
    // limiter escribe la respuesta directamente y nunca pasa por ese middleware.
    options.OnRejected = async (contexto, token) =>
    {
        contexto.HttpContext.Response.ContentType = "application/json";
        var respuesta = RespuestaApi<object>.Error("Demasiados intentos. Espera un minuto e intenta de nuevo.");
        await contexto.HttpContext.Response.WriteAsync(
            JsonSerializer.Serialize(respuesta, opcionesJsonRespuestaApi), token);
    };

    options.AddPolicy("login", httpContext => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "desconocido",
        factory: _ => new FixedWindowRateLimiterOptions
        {
            Window = TimeSpan.FromMinutes(1),
            PermitLimit = 5,
            QueueLimit = 0
        }));
});

// ---- CORS (frontend en Azure Static Web Apps / Vite dev server) ----
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(origenesPermitidos)
            .AllowAnyHeader()
            .AllowAnyMethod());
});

// ---- Servicios de aplicación ----
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IVehiculoService, VehiculoService>();
builder.Services.AddScoped<ICargaService, CargaService>();
builder.Services.AddScoped<ICatalogoService, CatalogoService>();
builder.Services.AddScoped<IEstadisticaService, EstadisticaService>();

// ---- Controllers + filtros globales (auditoría y validación de TokenVersion) ----
builder.Services.AddControllers(options =>
{
    options.Filters.Add<AuditoriaActionFilter>();
    options.Filters.Add<ValidarTokenVersionFilter>();
})
.ConfigureApiBehaviorOptions(options =>
{
    // El 400 automático de [ApiController] corre antes que cualquier ActionFilter,
    // así que reemplaza la respuesta para usar el sobre RespuestaApi<T> y auditar el rechazo.
    options.InvalidModelStateResponseFactory = ValidacionModeloResponseFactory.Crear;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresa el token JWT con el prefijo 'Bearer '."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ManejoErroresMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
