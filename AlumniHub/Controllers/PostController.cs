using AlumniHub.Data;
using AlumniHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlumniHub.Controllers
{
    [Authorize]
    public class PostController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PostController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Post?filter=all or /Post?filter=my
        public async Task<IActionResult> Index(string filter = "all")
        {
            var currentUserId = _userManager.GetUserId(User);
            ViewBag.CurrentUserId = currentUserId;
            ViewBag.ActiveFilter = filter;

            // Base query: order newest first
            var query = _context.Posts
                .Include(p => p.User)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .OrderByDescending(p => p.CreatedAt);

            // Total counts for the tabs
            ViewBag.AllPostsCount = await query.CountAsync();
            ViewBag.MyPostsCount = await query.CountAsync(p => p.UserId == currentUserId);

            // Filter if "My Posts" tab is clicked
            List<Post> posts;
            if (filter == "my")
            {
                posts = await query.Where(p => p.UserId == currentUserId).ToListAsync();
            }
            else
            {
                posts = await query.ToListAsync();
            }

            // Load student and alumni profiles to display author names and avatars
            var studentProfiles = await _context.StudentProfiles.ToDictionaryAsync(s => s.UserId, s => s);
            var alumniProfiles = await _context.AlumniProfiles.ToDictionaryAsync(a => a.UserId, a => a);

            ViewBag.StudentProfiles = studentProfiles;
            ViewBag.AlumniProfiles = alumniProfiles;

            return View(posts);
        }

        // POST: /Post/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string content, IFormFile? postImage)
        {
            if (string.IsNullOrWhiteSpace(content) && (postImage == null || postImage.Length == 0))
            {
                TempData["ErrorMessage"] = "Post content or an image is required.";
                return RedirectToAction(nameof(Index));
            }

            var userId = _userManager.GetUserId(User);
            var post = new Post
            {
                UserId = userId!,
                Content = content ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            // Handle optional image upload
            if (postImage != null && postImage.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(postImage.FileName);
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var filePath = Path.Combine(uploadPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await postImage.CopyToAsync(stream);
                }
                post.Image = fileName;
            }

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Post published successfully!";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Post/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, string filter = "all")
        {
            var currentUserId = _userManager.GetUserId(User);
            var post = await _context.Posts
                .Include(p => p.Comments)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
                return NotFound();

            // Only author can delete their post
            if (post.UserId != currentUserId)
                return Forbid();

            // Delete associated image file from disk if present
            if (!string.IsNullOrEmpty(post.Image))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", post.Image);
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
            }

            _context.Comments.RemoveRange(post.Comments);
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Post deleted successfully.";
            return RedirectToAction(nameof(Index), new { filter = filter });
        }

        // POST: /Post/AddComment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int postId, string content, string filter = "all")
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return RedirectToAction(nameof(Index), new { filter = filter });
            }

            var userId = _userManager.GetUserId(User);
            var comment = new Comment
            {
                PostId = postId,
                UserId = userId!,
                Content = content.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { filter = filter });
        }

        // POST: /Post/DeleteComment/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int id, string filter = "all")
        {
            var currentUserId = _userManager.GetUserId(User);
            var comment = await _context.Comments.FindAsync(id);

            if (comment == null)
                return NotFound();

            // Only comment author can delete it
            if (comment.UserId != currentUserId)
                return Forbid();

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { filter = filter });
        }
    }
}