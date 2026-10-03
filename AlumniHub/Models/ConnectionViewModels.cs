namespace AlumniHub.Models
{
    public class MemberDirectoryItem
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // "Student" or "Alumni"
        public string Department { get; set; } = string.Empty;
        public string? Details { get; set; } // "Batch 2024" or "Software Engineer at TCS"
        public string? City { get; set; }
        public string? ProfileImage { get; set; }
        public string ConnectionStatus { get; set; } = "None"; // "None", "PendingSent", "PendingReceived", "Connected"
        public int? ConnectionId { get; set; }
    }

    public class ConnectionIndexViewModel
    {
        public string ActiveTab { get; set; } = "directory";
        public string SearchTerm { get; set; } = string.Empty;
        public List<MemberDirectoryItem> DirectoryMembers { get; set; } = new();
        public List<MemberDirectoryItem> PendingReceivedRequests { get; set; } = new();
        public List<MemberDirectoryItem> PendingSentRequests { get; set; } = new();
        public List<MemberDirectoryItem> MyConnections { get; set; } = new();
    }
}