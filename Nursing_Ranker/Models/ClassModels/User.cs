using System.ComponentModel.DataAnnotations;

namespace Nursing_Ranker.Models.ClassModels
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public required string FirstName { get; set; }

        [Required, MaxLength(50)]
        public required string LastName { get; set; }

        [Required, EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string PasswordHash { get; set; }

        [Required]
        public required string FavColor { get; set; } = "white";

        public string? ProfilePicturePath { get; set; }
    }
}






