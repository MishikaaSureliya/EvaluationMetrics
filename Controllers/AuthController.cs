using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EvolutionMetrics.Data;
using EvolutionMetrics.Models;
using EvolutionMetrics.Services;
using EvolutionMetrics.DTOs;

namespace EvolutionMetrics.Controllers
{
    [AllowAnonymous] // ✅ important
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwt;

        public AuthController(AppDbContext context, JwtService jwt)
        {
            _context = context;
            _jwt = jwt;
        }

        // 🔹 PAGE: Login
        public IActionResult Login()
        {
            return View();
        }

        // 🔹 PAGE: Register
        public IActionResult Register()
        {
            return View();
        }

        // 🔹 API: Register
        [HttpPost]
        [Route("api/auth/register")]
        public IActionResult RegisterUser([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (_context.Users.Any(u => u.Email == dto.Email || u.Name == dto.Name))
                return BadRequest("User already exists");

            string hash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                PasswordHash = hash,

                CreatedBy = dto.Name,              // ✅ FIX HERE
                CreatedDate = DateTime.UtcNow
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok("Registered successfully");
        }

        // 🔹 API: Login
        [HttpPost]
        [Route("api/auth/login")]
        public IActionResult LoginUser(string email, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return Unauthorized("Invalid credentials");

            var token = _jwt.GenerateToken(user);

            return Ok(new { token });
        }
    }
}