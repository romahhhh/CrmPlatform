using CRMService.Cache;
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
    public class SessionServiceTests
    {
        private static AppDbContext CreateInMemoryDb()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task CreateAsync_InvalidatesCacheForClient()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var clientId = Guid.NewGuid();

            using var db = CreateInMemoryDb();
            db.Clients.Add(new Client
            {
                Id = clientId,
                Name = "Клиент",
                Email = "c@c.com",
                Phone = "1",
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();

            var cacheMock = new Mock<ISessionCache>();
            var publisherMock = new Mock<ICrmEventPublisher>();
            var service = new SessionService(db, cacheMock.Object, publisherMock.Object);

            var request = new SessionCreateRequest(
                clientId,
                DateTime.UtcNow.AddDays(1),
                60,
                SessionStatus.Planned,
                "Тест");

            // Act
            await service.CreateAsync(userId, request);

            // Assert
            cacheMock.Verify(c => c.InvalidateAsync(clientId), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_InvalidatesCache()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            var sessionId = Guid.NewGuid();

            using var db = CreateInMemoryDb();
            db.Clients.Add(new Client
            {
                Id = clientId,
                Name = "Клиент",
                Email = "c@c.com",
                Phone = "1",
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            });
            db.Sessions.Add(new Session
            {
                Id = sessionId,
                ClientId = clientId,
                ScheduledAt = DateTime.UtcNow.AddDays(1),
                DurationInMinutes = 60,
                Status = SessionStatus.Planned,
                Notes = ""
            });
            await db.SaveChangesAsync();

            var cacheMock = new Mock<ISessionCache>();
            var publisherMock = new Mock<ICrmEventPublisher>();
            var service = new SessionService(db, cacheMock.Object, publisherMock.Object);

            var request = new SessionUpdateRequest(
                DateTime.UtcNow.AddDays(2),
                90,
                SessionStatus.Completed,
                "Обновлено");

            // Act
            await service.UpdateAsync(userId, sessionId, request);

            // Assert
            cacheMock.Verify(c => c.InvalidateAsync(clientId), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_InvalidatesCache()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            var sessionId = Guid.NewGuid();

            using var db = CreateInMemoryDb();
            db.Clients.Add(new Client
            {
                Id = clientId,
                Name = "Клиент",
                Email = "c@c.com",
                Phone = "1",
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            });
            db.Sessions.Add(new Session
            {
                Id = sessionId,
                ClientId = clientId,
                ScheduledAt = DateTime.UtcNow.AddDays(1),
                DurationInMinutes = 60,
                Status = SessionStatus.Planned,
                Notes = ""
            });
            await db.SaveChangesAsync();

            var cacheMock = new Mock<ISessionCache>();
            var publisherMock = new Mock<ICrmEventPublisher>();
            var service = new SessionService(db, cacheMock.Object, publisherMock.Object);

            // Act
            await service.DeleteAsync(userId, sessionId);

            // Assert
            cacheMock.Verify(c => c.InvalidateAsync(clientId), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_Throws_WhenClientBelongsToAnotherUser()
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

            var cacheMock = new Mock<ISessionCache>();
            var publisherMock = new Mock<ICrmEventPublisher>();
            var service = new SessionService(db, cacheMock.Object, publisherMock.Object);

            var request = new SessionCreateRequest(
                clientId, DateTime.UtcNow.AddDays(1), 60, SessionStatus.Planned, "");

            // Act
            var act = async () => await service.CreateAsync(attackerId, request);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>();
        }
    }
}
