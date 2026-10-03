namespace AlumniHub.Models
{
    public class Connection
    {
        public int Id { get; set; }

        // User who sent the request
        public string SenderId { get; set; } = string.Empty;
        public ApplicationUser? Sender { get; set; }

        // User who received the request
        public string ReceiverId { get; set; } = string.Empty;
        public ApplicationUser? Receiver { get; set; }

        // "Pending", "Accepted", or "Rejected"
        public string Status { get; set; } = "Pending";
    }
}