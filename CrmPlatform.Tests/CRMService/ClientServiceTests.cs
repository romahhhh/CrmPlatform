using CRMService.Data;
using CRMService.DTOs;
using CRMService.Messaging;
using CRMService.Models;
using CRMService.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmPlatform.Tests.CRMService
{
    public class ClientServiceTests
    {
        private static AppDbContext CreateInMemoryDb()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        private static ClientService CreateService(AppDbContext db, ICrmEventPublisher? publisher = null)
        {
            publisher ??= new Mock<ICrmEventPublisher>().Object;
            return new ClientService(db, publisher);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsOnlyClientsOfGivenUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();

            using var db = CreateInMemoryDb();
            db.Clients.AddRange(
                new Client { Id = Guid.NewGuid(), Name = "A", Email = "a@a.com", Phone = "1", UserId = userId, CreatedAt = DateTime.UtcNow },
                new Client { Id = Guid.NewGuid(), Name = "B", Email = "b@b.com", Phone = "2", UserId = userId, CreatedAt = DateTime.UtcNow },
                new Client { Id = Guid.NewGuid(), Name = "C", Email = "c@c.com", Phone = "3", UserId = otherUserId, CreatedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();

            var service = CreateService(db);

            // Act
            var result = await service.GetAllAsync(userId);

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(c => c.Name == "A" || c.Name == "B");
        }

        [Fact]
        public async Task GetByIdAsync_Throws_WhenClientBelongsToAnotherUser()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var attackerId = Guid.NewGuid();
            var clientId = Guid.NewGuid();

            using var db = CreateInMemoryDb();
            db.Clients.Add(new Client
            {
                Id = clientId,
                Name = "Secret",
                Email = "s@s.com",
                Phone = "1",
                UserId = ownerId,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();

            var service = CreateService(db);

            // Act
            var act = async () => await service.GetByIdAsync(attackerId, clientId);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task UpdateAsync_Throws_WhenClientBelongsToAnotherUser()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var attackerId = Guid.NewGuid();
            var clientId = Guid.NewGuid();

            using var db = CreateInMemoryDb();
            db.Clients.Add(new Client
            {
                Id = clientId,
                Name = "Secret",
                Email = "s@s.com",
                Phone = "1",
                UserId = ownerId,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();

            var service = CreateService(db);
            var request = new ClientUpdateRequest("Hacked", "h@h.com", "999");

            // Act
            var act = async () => await service.UpdateAsync(attackerId, clientId, request);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task DeleteAsync_Throws_WhenClientBelongsToAnotherUser()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var attackerId = Guid.NewGuid();
            var clientId = Guid.NewGuid();

            using var db = CreateInMemoryDb();
            db.Clients.Add(new Client
            {
                Id = clientId,
                Name = "Secret",
                Email = "s@s.com",
                Phone = "1",
                UserId = ownerId,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();

            var service = CreateService(db);

            // Act
            var act = async () => await service.DeleteAsync(attackerId, clientId);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task CreateAsync_PublishesClientCreatedEvent()
        {
            // Arrange
            var userId = Guid.NewGuid();
            using var db = CreateInMemoryDb();
            var publisherMock = new Mock<ICrmEventPublisher>();
            var service = CreateService(db, publisherMock.Object);

            var request = new ClientCreateRequest("Анна", "anna@example.com", "+375291234567");

            // Act
            await service.CreateAsync(userId, request);

            // Assert
            publisherMock.Verify(p => p.PublishClientCreatedAsync(
                It.Is<Shared.Events.ClientCreatedEvent>(e =>
                    e.Name == "Анна" && e.Email == "anna@example.com"),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
