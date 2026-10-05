using MassTransit;
using Microsoft.Extensions.Options;
using NotificationService.Consumers;
using NotificationService.Services;
using Serilog;
using Shared.Events;
using Shared.Settings;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();


builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection(RabbitMqOptions.SectionName));

builder.Services.AddScoped<IConsoleNotificationSender, ConsoleNotificationSender>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<UserRegisteredConsumer>();
    x.AddConsumer<ClientCreatedConsumer>();
    x.AddConsumer<SessionPlannedConsumer>();

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

        cfg.Message<UserRegisteredEvent>(e => e.SetEntityName("crm.events"));
        cfg.Send<UserRegisteredEvent>(s =>
            s.UseRoutingKeyFormatter(_ => "user.registered"));

        cfg.Message<SessionPlannedEvent>(e => e.SetEntityName(RabbitMqConstants.Exchange));
        cfg.Send<SessionPlannedEvent>(s =>
            s.UseRoutingKeyFormatter(_ => RabbitMqConstants.SessionPlannedRoutingKey));

        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

//app.MapControllers();

app.Run();
