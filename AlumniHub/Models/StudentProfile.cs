namespace AlumniHub.Models
{
    public class StudentProfile
    {
        public int Id { get; set; }

        // Links this profile to the user account
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public string FullName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public int BatchYear { get; set; }
        public string Bio { get; set; } = string.Empty;

        // Optional profile image file name
        public string? ProfileImage { get; set; }
    }
}