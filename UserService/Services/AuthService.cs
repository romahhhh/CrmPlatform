using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Shared.Events;
using UserService.Data;
using UserService.Dtos;
using UserService.Messaging;
using UserService.Models;

namespace UserService.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly IPasswordHasher _hasher;
        private readonly IJwtTokenGenerator _jwt;
        private readonly IUserEventPublisher _eventPublisher;
        public AuthService(AppDbContext db, IPasswordHasher hasher, IJwtTokenGenerator jwt, IUserEventPublisher eventPublisher)
        {
            _db = db;
            _hasher = hasher;
            _jwt = jwt;
            _eventPublisher = eventPublisher;
        }
        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            var email = request.Email.Trim().ToLowerInvariant();

            if (await _db.Users.AnyAsync(u => u.Email == email))
                throw new InvalidOperationException("Пользователь с таким email уже существует.");

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                Name = request.Name.Trim(),
                PasswordHash = _hasher.Hash(request.Password),
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            await _eventPublisher.PublishUserRegisteredAsync(new UserRegisteredEvent(
                user.Id,
                user.Email,
                user.Name,
                user.CreatedAt));

            return new AuthResponse(user.Id, user.Email, user.Name, _jwt.Generate(user));
        }
        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var email = request.Email.Trim().ToLowerInvariant();

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user is null)
                throw new InvalidOperationException("Неверный email или пароль.");

            if (!_hasher.Verify(request.Password, user.PasswordHash))
                throw new InvalidOperationException("Неверный email или пароль.");

            return new AuthResponse(user.Id, user.Email, user.Name, _jwt.Generate(user));
        }

        public async Task DeleteAsync(Guid UserId)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == UserId)
                ?? throw new InvalidOperationException("Пользователь не найден.");
            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
        }

        public async Task<AuthResponse> UpdateAsync(Guid UserId, UpdateUserRequest request)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == UserId)
                ?? throw new InvalidOperationException("Пользователь не найден.");

            user.Email = request.Email.Trim().ToLowerInvariant();
            user.PasswordHash = _hasher.Hash(request.Password);
            user.Name = request.Name.Trim();

            await _db.SaveChangesAsync();

            return new AuthResponse(user.Id, user.Email, user.Name, _jwt.Generate(user));
        }
    }
}
