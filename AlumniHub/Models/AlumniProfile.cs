namespace AlumniHub.Models
{
    public class AlumniProfile
    {
        public int Id { get; set; }

        // Links this profile to the user account
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public string FullName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public int GraduationYear { get; set; }
        public string CurrentCompany { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;

        // Optional profile image file name
        public string? ProfileImage { get; set; }
    }
}