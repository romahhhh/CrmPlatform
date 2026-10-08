using MassTransit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Testcontainers.PostgreSql;

namespace CrmPlatform.Tests.Infrastructure
{
    public class CrmWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16")
            .WithDatabase("crm_test_db")
            .WithUsername("test")
            .WithPassword("test")
            .Build();

        public async Task InitializeAsync()
        {
            await _postgres.StartAsync();
        }

        public new async Task DisposeAsync()
        {
            await _postgres.DisposeAsync();
            await base.DisposeAsync();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = _postgres.GetConnectionString()
                });
            });

            builder.ConfigureTestServices(services =>
            {
                // 1. Удаляем реальный MassTransit (RabbitMQ)
                var massTransitDescriptors = services
                    .Where(d => d.ServiceType.FullName != null &&
                                d.ServiceType.FullName.Contains("MassTransit"))
                    .ToList();
                foreach (var descriptor in massTransitDescriptors)
                    services.Remove(descriptor);

                // 2. Ставим MassTransit Test Harness (in-memory, без RabbitMQ)
                services.AddMassTransitTestHarness();

                // 3. Тестовая аутентификация
                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
            });
        }
    }
}
