using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EvolutionMetrics.Data;
using EvolutionMetrics.Models;
using EvolutionMetrics.Services;
using EvolutionMetrics.DTOs;

namespace EvolutionMetrics.Controllers
{
    [AllowAnonymous]
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AppDbContext context, IJwtService jwtService, ILogger<AuthController> logger)
        {
            _context = context;
            _jwtService = jwtService;
            _logger = logger;
        }

        /// <summary>
        /// Returns the Login page view.
        /// </summary>
        public IActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// Returns the Register page view.
        /// </summary>
        public IActionResult Register()
        {
            return View();
        }

        /// <summary>
        /// Registers a new user account with the provided details.
        /// </summary>
        [HttpPost]
        [Route("api/auth/register")]
        public IActionResult RegisterUser([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (_context.Users.Any(u => u.Email == dto.Email || u.Name == dto.Name))
            {
                return BadRequest("User already exists");
            }

            try
            {
                string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

                var user = new User
                {
                    Name = dto.Name,
                    Email = dto.Email,
                    PasswordHash = passwordHash,
                    CreatedBy = dto.Name,
                    CreatedDate = DateTime.UtcNow
                };

                _context.Users.Add(user);
                _context.SaveChanges();

                _logger.LogInformation("User registered successfully: {Name}", dto.Name);

                return Ok("Registered successfully");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Failed to register user: {Name}", dto.Name);
                throw;
            }
        }

        /// <summary>
        /// Authenticates a user and returns a signed JWT token.
        /// </summary>
        [HttpPost]
        [Route("api/auth/login")]
        public IActionResult LoginUser(string email, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return Unauthorized("Invalid credentials");
            }

            var token = _jwtService.GenerateToken(user);

            _logger.LogInformation("User logged in: {Email}", email);

            return Ok(new { token });
        }
    }
}