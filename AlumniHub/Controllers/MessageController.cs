using AlumniHub.Data;
using AlumniHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlumniHub.Controllers
{
    [Authorize]
    public class MessageController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MessageController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Message/Chat?userId=...
        public async Task<IActionResult> Chat(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Index", "Connection");

            var currentUserId = _userManager.GetUserId(User);

            // Fetch the person we are chatting with
            var student = await _context.StudentProfiles.FirstOrDefaultAsync(s => s.UserId == userId);
            var alumni = await _context.AlumniProfiles.FirstOrDefaultAsync(a => a.UserId == userId);

            ViewBag.PartnerId = userId;
            ViewBag.PartnerName = student?.FullName ?? alumni?.FullName ?? "User";
            ViewBag.PartnerRole = student != null ? "Student" : (alumni != null ? "Alumni" : "Member");
            ViewBag.PartnerImage = student?.ProfileImage ?? alumni?.ProfileImage;
            ViewBag.CurrentUserId = currentUserId;

            // ✅ Mark all unread messages sent by this partner as READ
            var unreadMessages = await _context.Messages
                .Where(m => m.SenderId == userId && m.ReceiverId == currentUserId && !m.IsRead)
                .ToListAsync();

            if (unreadMessages.Any())
            {
                foreach (var msg in unreadMessages)
                {
                    msg.IsRead = true;
                }
                await _context.SaveChangesAsync();
            }

            // Load full conversation history
            var messages = await _context.Messages
                .Where(m => (m.SenderId == currentUserId && m.ReceiverId == userId) ||
                            (m.SenderId == userId && m.ReceiverId == currentUserId))
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            return View(messages);
        }

        // POST: /Message/Send
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(string receiverId, string content)
        {
            var currentUserId = _userManager.GetUserId(User);

            if (!string.IsNullOrWhiteSpace(content) && !string.IsNullOrEmpty(receiverId))
            {
                var message = new Message
                {
                    SenderId = currentUserId!,
                    ReceiverId = receiverId,
                    Content = content.Trim(),
                    SentAt = DateTime.Now,
                    IsRead = false // New message is unread by default!
                };

                _context.Messages.Add(message);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Chat), new { userId = receiverId });
        }
    }
}