using System.ComponentModel.DataAnnotations;

namespace AlumniHub.Models
{
    public class StudentProfile
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full Name is required")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required")]
        public string Department { get; set; } = string.Empty;

        [Required(ErrorMessage = "Batch Year is required")]
        [Range(2000, 2100, ErrorMessage = "Enter a valid year")]
        public int BatchYear { get; set; }

        public string? Bio { get; set; }
        public string? ProfileImage { get; set; }
        public ApplicationUser? User { get; set; }
    }
}