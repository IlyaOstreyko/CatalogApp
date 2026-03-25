using CatalogApp.Infrastructure.Interfaces;
using CatalogApp.Shared.Api;
using CatalogApp.Shared.Auth;
using CatalogApp.Shared.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CatalogApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUnitOfWork _uow;
        private readonly JwtSettings _jwt;
        private readonly IDbConnectionFactory _connFactory;

        public AuthController(IUnitOfWork uow, JwtSettings jwt, IDbConnectionFactory connFactory)
        {
            _uow = uow;
            _jwt = jwt;
            _connFactory = connFactory;
        }
        [HttpGet("email-exists")]
        public async Task<bool> CheckEmailExistsAsync(string email)
        {
            return await _uow.Users.CheckEmailExistsAsync(email);
        }
        [HttpGet("username-exists")]
        public async Task<bool> CheckUsernameExistsAsync(string username)
        {
            return await _uow.Users.CheckUsernameExistsAsync(username);
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            var existing = await _uow.Users.GetByUsernameAsync(req.Username);
            if (existing != null) return BadRequest("User already exists");

            var userDto = new UserDto { Username = req.Username, UserEmail = req.Email };
            var hash = BCrypt.Net.BCrypt.HashPassword(req.Password);
            await _uow.Users.CreateAsync(userDto, hash);
            _uow.Commit();
            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            var user = await _uow.Users.GetByUsernameAsync(req.Username);
            if (user == null) return Unauthorized();

            // Получаем хеш пароля
            var hash = await _uow.Users.GetPasswordHashByUsernameAsync(req.Username);
            if (hash == null || !BCrypt.Net.BCrypt.Verify(req.Password, hash)) return Unauthorized();

            var token = GenerateToken(user);
            return Ok(new { token });
        }

        private string GenerateToken(UserDto user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("id", user.Id.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwt.ExpireMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
