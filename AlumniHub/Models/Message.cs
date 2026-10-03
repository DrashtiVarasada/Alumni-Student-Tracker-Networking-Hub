namespace AlumniHub.Models
{
    public class Message
    {
        public int Id { get; set; }

        // Who sent the message
        public string SenderId { get; set; } = string.Empty;
        public ApplicationUser? Sender { get; set; }

        // Who received the message
        public string ReceiverId { get; set; } = string.Empty;
        public ApplicationUser? Receiver { get; set; }

        public string Content { get; set; } = string.Empty;

        public DateTime SentAt { get; set; } = DateTime.Now;
    }
}