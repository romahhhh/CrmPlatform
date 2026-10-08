using CrmPlatform.Tests.Infrastructure;
using CRMService.DTOs;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace CrmPlatform.Tests.Integration
{
    public class ClientsControllerIntegrationTests : IClassFixture<CrmWebApplicationFactory>
    {
        private readonly CrmWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public ClientsControllerIntegrationTests(CrmWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }
        /// <summary>
        /// POST /api/clients реально создаёт клиента в реальной БД.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Post_Clients_CreatesClientInDatabase()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderName, userId.ToString());

            var request = new ClientCreateRequest(
                "Интеграционный Тест",
                "integration@test.com",
                "+375291234567");

            // Act
            var response = await _client.PostAsJsonAsync("/api/clients", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var body = await response.Content.ReadFromJsonAsync<ClientResponse>();
            body.Should().NotBeNull();
            body!.Name.Should().Be("Интеграционный Тест");
            body.Email.Should().Be("integration@test.com");
            body.Phone.Should().Be("+375291234567");
        }
        /// <summary>
        /// GET /api/clients возвращает только клиентов текущего пользователя. Защита через HTTP от IDOR
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Get_Clients_ReturnsOnlyCurrentUserClients()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();

            // Создаём клиента от имени userId
            _client.DefaultRequestHeaders.Remove(TestAuthHandler.HeaderName);
            _client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderName, userId.ToString());
            await _client.PostAsJsonAsync("/api/clients",
                new ClientCreateRequest("Наш Клиент", "ours@test.com", "+375291111111"));

            // Создаём клиента от имени otherUserId
            _client.DefaultRequestHeaders.Remove(TestAuthHandler.HeaderName);
            _client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderName, otherUserId.ToString());
            await _client.PostAsJsonAsync("/api/clients",
                new ClientCreateRequest("Чужой Клиент", "theirs@test.com", "+375292222222"));

            // Act — запрашиваем список от имени userId
            _client.DefaultRequestHeaders.Remove(TestAuthHandler.HeaderName);
            _client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderName, userId.ToString());
            var response = await _client.GetAsync("/api/clients");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var clients = await response.Content.ReadFromJsonAsync<List<ClientResponse>>();
            clients.Should().HaveCount(1);
            clients!.Single().Name.Should().Be("Наш Клиент");
        }
    }
}
