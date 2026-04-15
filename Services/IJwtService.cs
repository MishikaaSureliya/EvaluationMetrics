using EvolutionMetrics.Models;

namespace EvolutionMetrics.Services
{
    /// <summary>
    /// Contract for JWT token generation.
    /// </summary>
    public interface IJwtService
    {
        /// <summary>
        /// Generates a signed JWT token for the specified user.
        /// </summary>
        string GenerateToken(User user);
    }
}
