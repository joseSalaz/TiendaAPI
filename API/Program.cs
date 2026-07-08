using AutoMapper;
using DBModel.DBModels;
using IoC;
using IService.FacturacionElectronica;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Models.ApisPeru;
using QuestPDF.Infrastructure;
using Service.FacturacionElectronica;
using System.Text;
using UtilMaper;

var builder = WebApplication.CreateBuilder(args);

#region CORS

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

#endregion

#region DbContext

builder.Services.AddDbContext<_TiendaDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"));
});

#endregion

#region APIS PERU
builder.Services.Configure<ApisPeruOptions>(
    builder.Configuration.GetSection("ApisPeru")
);

builder.Services.AddHttpClient<IApisPeruFacturacionService, ApisPeruFacturacionService>((serviceProvider, client) =>
{
    var options = serviceProvider
        .GetRequiredService<Microsoft.Extensions.Options.IOptions<ApisPeruOptions>>()
        .Value;

    client.BaseAddress = new Uri(options.BaseUrl);
});
#endregion

#region JWT

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidAudience =
                    builder.Configuration["Jwt:Audience"],

                ValidIssuer =
                    builder.Configuration["Jwt:Issuer"],

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            builder.Configuration["Jwt:Key"]!))
            };
    });

#endregion

#region Controllers
builder.Services.AddHttpClient();

builder.Services.AddControllers();

#endregion

#region Swagger

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SISTEMA DE MINIMARKET",
        Version = "v1",
        Description =
            "Documentación de los servicios para el sistema de minimarket",
        Contact = new OpenApiContact
        {
            Name = "José Salazar"
        }
    });

    var xmlFilename =
        $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";

    c.IncludeXmlComments(
        Path.Combine(
            AppContext.BaseDirectory,
            xmlFilename));

    c.AddSecurityDefinition("Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description =
                "Ingrese el token JWT. Ejemplo: Bearer {token}"
        });

    c.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference =
                        new OpenApiReference
                        {
                            Type =
                                ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                },
                Array.Empty<string>()
            }
        });
});

#endregion

#region AutoMapper

builder.Services.AddSingleton<IMapper>(sp =>
{
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

    var mapperConfig = new MapperConfiguration(
        cfg => cfg.AddMaps(typeof(AutoMapperProfiles).Assembly),
        loggerFactory
    );

    return new Mapper(mapperConfig);
});

#endregion

#region QUESTPDF
QuestPDF.Settings.License = LicenseType.Community;
#endregion

#region Dependency Injection

builder.Services.AddDependencyInjection();

#endregion

var app = builder.Build();

#region Pipeline

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

#endregion

app.Run();