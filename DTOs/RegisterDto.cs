using System.ComponentModel.DataAnnotations;

namespace EvolutionMetrics.DTOs
{
    public class RegisterDto
    {
        [Required]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "Name should contain only characters")]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(8)]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$",
            ErrorMessage = "Password must contain 1 uppercase, 1 number, 1 special character")]
        public string Password { get; set; }

        // ✅ NEW FIELD
        [Required]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }
    }
}
