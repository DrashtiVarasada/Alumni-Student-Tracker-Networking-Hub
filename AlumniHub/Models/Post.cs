namespace AlumniHub.Models
{
    public class Post
    {
        public int Id { get; set; }

        // Who created this post
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public string Content { get; set; } = string.Empty;

        // Optional image attached to post
        public string? Image { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // A post can have many comments
        public List<Comment> Comments { get; set; } = new List<Comment>();
    }
}