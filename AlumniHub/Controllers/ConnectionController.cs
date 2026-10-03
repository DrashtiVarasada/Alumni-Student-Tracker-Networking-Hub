using AlumniHub.Data;
using AlumniHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlumniHub.Controllers
{
    [Authorize]
    public class ConnectionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ConnectionController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Connection
        public async Task<IActionResult> Index(string? search, string tab = "directory")
        {
            var currentUserId = _userManager.GetUserId(User);

            // Fetch all connection records involving the current user
            var userConnections = await _context.Connections
                .Where(c => c.SenderId == currentUserId || c.ReceiverId == currentUserId)
                .ToListAsync();

            // Load all student and alumni profiles
            var students = await _context.StudentProfiles.ToListAsync();
            var alumni = await _context.AlumniProfiles.ToListAsync();

            // Load unread messages sent to the current user
            var unreadMessages = await _context.Messages
                .Where(m => m.ReceiverId == currentUserId && !m.IsRead)
                .ToListAsync();

            var allMembers = new List<MemberDirectoryItem>();

            // Map Students
            foreach (var s in students)
            {
                if (s.UserId == currentUserId) continue;

                allMembers.Add(new MemberDirectoryItem
                {
                    UserId = s.UserId,
                    FullName = s.FullName,
                    Role = "Student",
                    Department = s.Department,
                    Details = $"Batch {s.BatchYear}",
                    ProfileImage = s.ProfileImage
                });
            }

            // Map Alumni
            foreach (var a in alumni)
            {
                if (a.UserId == currentUserId) continue;

                string details = string.Empty;
                if (!string.IsNullOrEmpty(a.Designation) && !string.IsNullOrEmpty(a.CurrentCompany))
                    details = $"{a.Designation} at {a.CurrentCompany}";
                else if (!string.IsNullOrEmpty(a.CurrentCompany))
                    details = a.CurrentCompany;
                else
                    details = $"Graduated {a.GraduationYear}";

                allMembers.Add(new MemberDirectoryItem
                {
                    UserId = a.UserId,
                    FullName = a.FullName,
                    Role = "Alumni",
                    Department = a.Department,
                    Details = details,
                    ProfileImage = a.ProfileImage
                });
            }

            // Match connection statuses and check if unread
            foreach (var m in allMembers)
            {
                var conn = userConnections.FirstOrDefault(c =>
                    (c.SenderId == currentUserId && c.ReceiverId == m.UserId) ||
                    (c.ReceiverId == currentUserId && c.SenderId == m.UserId));

                if (conn != null)
                {
                    m.ConnectionId = conn.Id;
                    if (conn.Status == "Accepted")
                    {
                        m.ConnectionStatus = "Connected";

                        // If this person sent you ANY message that is still unread (!IsRead)
                        if (unreadMessages.Any(msg => msg.SenderId == m.UserId))
                        {
                            m.City = "new_msg";
                        }
                    }
                    else if (conn.Status == "Pending")
                    {
                        m.ConnectionStatus = conn.SenderId == currentUserId ? "PendingSent" : "PendingReceived";
                    }
                    else
                    {
                        m.ConnectionStatus = "None";
                    }
                }
            }

            // Filter by search query if provided
            var filteredMembers = allMembers;
            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = search.Trim().ToLower();
                filteredMembers = allMembers.Where(m =>
                    m.FullName.ToLower().Contains(q) ||
                    m.Department.ToLower().Contains(q) ||
                    (m.Details != null && m.Details.ToLower().Contains(q))
                ).ToList();
            }

            var viewModel = new ConnectionIndexViewModel
            {
                ActiveTab = tab,
                SearchTerm = search ?? string.Empty,
                DirectoryMembers = filteredMembers.OrderBy(m => m.FullName).ToList(),
                PendingReceivedRequests = allMembers.Where(m => m.ConnectionStatus == "PendingReceived").ToList(),
                PendingSentRequests = allMembers.Where(m => m.ConnectionStatus == "PendingSent").ToList(),
                MyConnections = allMembers.Where(m => m.ConnectionStatus == "Connected").ToList()
            };

            return View(viewModel);
        }

        // POST: /Connection/SendRequest
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendRequest(string receiverId)
        {
            var currentUserId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(receiverId) || receiverId == currentUserId)
                return RedirectToAction(nameof(Index));

            var existing = await _context.Connections
                .FirstOrDefaultAsync(c =>
                    (c.SenderId == currentUserId && c.ReceiverId == receiverId) ||
                    (c.ReceiverId == currentUserId && c.SenderId == receiverId));

            if (existing == null)
            {
                var connection = new Connection
                {
                    SenderId = currentUserId!,
                    ReceiverId = receiverId,
                    Status = "Pending"
                };
                _context.Connections.Add(connection);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Connection request sent!";
            }

            return RedirectToAction(nameof(Index), new { tab = "directory" });
        }

        // POST: /Connection/AcceptRequest
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptRequest(int connectionId)
        {
            var currentUserId = _userManager.GetUserId(User);
            var connection = await _context.Connections.FindAsync(connectionId);

            if (connection != null && connection.ReceiverId == currentUserId)
            {
                connection.Status = "Accepted";
                _context.Connections.Update(connection);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Connection request accepted!";
            }

            return RedirectToAction(nameof(Index), new { tab = "requests" });
        }

        // POST: /Connection/RejectRequest
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectRequest(int connectionId)
        {
            var currentUserId = _userManager.GetUserId(User);
            var connection = await _context.Connections.FindAsync(connectionId);

            if (connection != null && connection.ReceiverId == currentUserId)
            {
                _context.Connections.Remove(connection);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Connection request removed.";
            }

            return RedirectToAction(nameof(Index), new { tab = "requests" });
        }

        // POST: /Connection/RemoveConnection
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveConnection(int connectionId)
        {
            var currentUserId = _userManager.GetUserId(User);
            var connection = await _context.Connections.FindAsync(connectionId);

            if (connection != null && (connection.SenderId == currentUserId || connection.ReceiverId == currentUserId))
            {
                _context.Connections.Remove(connection);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Connection removed.";
            }

            return RedirectToAction(nameof(Index), new { tab = "connections" });
        }
    }
}