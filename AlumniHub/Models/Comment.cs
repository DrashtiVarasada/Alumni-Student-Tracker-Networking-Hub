namespace AlumniHub.Models
{
    public class Comment
    {
        public int Id { get; set; }

        // Which post this comment belongs to
        public int PostId { get; set; }
        public Post? Post { get; set; }

        // Who wrote this comment
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}