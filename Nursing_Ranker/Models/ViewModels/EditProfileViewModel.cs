using System.ComponentModel.DataAnnotations;

namespace Nursing_Ranker.Models
{
    public class EditProfileViewModel
    {
        public int UserId { get; set; } // To identify the user

        [Required(ErrorMessage = "First Name is required")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string? Email { get; set; }

        public IFormFile? ProfilePicture { get; set; }  // For new uploads

        public string? ExistingProfilePicture { get; set; } // Display current profile picture
    }
}
