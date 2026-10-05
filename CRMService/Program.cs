using CRMService.Cache;
using CRMService.Data;
using CRMService.Messaging;
using CRMService.Services;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Shared.Events;
using Shared.Settings;
using StackExchange.Redis;
using System.Text;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(JwtOptions.SectionName));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
        };

        options.MapInboundClaims = false;
    });

builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection(RabbitMqOptions.SectionName));

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        var options = context.GetRequiredService<IOptions<RabbitMqOptions>>().Value;

        cfg.Host(options.Host, options.Port, options.VirtualHost, h =>
        {
            h.Username(options.Username);
            h.Password(options.Password);
        });

        cfg.Message<ClientCreatedEvent>(e => e.SetEntityName(RabbitMqConstants.Exchange));
        cfg.Send<ClientCreatedEvent>(s =>
            s.UseRoutingKeyFormatter(_ => RabbitMqConstants.ClientCreatedRoutingKey));

        cfg.Message<SessionPlannedEvent>(e => e.SetEntityName(RabbitMqConstants.Exchange));
        cfg.Send<SessionPlannedEvent>(s =>
            s.UseRoutingKeyFormatter(_ => RabbitMqConstants.SessionPlannedRoutingKey));
    });
});

builder.Services.Configure<RedisOptions>(
    builder.Configuration.GetSection(RedisOptions.SectionName));

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var options = sp.GetRequiredService<IOptions<RedisOptions>>().Value;
    return ConnectionMultiplexer.Connect(options.ConnectionString);
});

builder.Services.AddScoped<ISessionCache, SessionCache>();
builder.Services.AddScoped<ISessionService, SessionService>();

builder.Services.AddScoped<ICrmEventPublisher, CrmEventPublisher>();

builder.Services.AddScoped<IClientService, ClientService>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Вставьте JWT-токен (без слова 'Bearer', Swagger добавит сам)"
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
});

var app = builder.Build();

// Автоматически применяем миграции при старте (для Docker)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
