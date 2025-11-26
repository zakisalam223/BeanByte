using forum_aspcore.Models;
using forum_aspcore.Stores;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace forum_aspcore.Controllers
{
    [Authorize]
    public class PrivateMessageController : Controller
    {
        private readonly MongoPrivateMessageStore _messageStore;
        private readonly MongoUserStore _userStore;

        public PrivateMessageController(MongoPrivateMessageStore messageStore, MongoUserStore userStore)
        {
            _messageStore = messageStore;
            _userStore = userStore;
        }

        // PrivateMessage/Inbox
        public async Task<IActionResult> Inbox()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var username = User.FindFirstValue(ClaimTypes.Name);

            // Console.WriteLine($"User ID: {userId}"); // for testing
            // Console.WriteLine($"Username: {username}"); // for testing

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Users");
            }

            var messages = await _messageStore.GetMessagesForUserAsync(userId);
            return View(messages);
        }

        // PrivateMessage/Sent
        public async Task<IActionResult> Sent()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Users");
            }

            var messages = await _messageStore.GetSentMessagesAsync(userId);
            return View("Inbox", messages);
        }

        // PrivateMessage/Send
        public async Task<IActionResult> Send(string recipientUsername, string content)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUsername = User.FindFirstValue(ClaimTypes.Name);
            var recipientUser = await _userStore.GetUserByUsernameAsync(recipientUsername);
            // Console.WriteLine($"Recipient username: {recipientUsername}"); // for testing
            // Console.WriteLine($"RecipientUser Obj: {recipientUser}"); // for testing

            if (recipientUser == null)
            {
                TempData["ErrorMessage"] = "User not found. Please try again.";
                return RedirectToAction("Inbox");
            }

            var recipientUserId = recipientUser.UserID;

            var message = new FPrivateMessage
            {
                RecipientID = recipientUserId,
                RecipientUsername = recipientUsername,

                SenderID = currentUserId,
                SenderUsername = currentUsername,

                Content = content,

                DateSent = DateTime.UtcNow,
                Status = false, // false = Unread
            };

            if (!String.IsNullOrEmpty(content))
            {
                await _messageStore.CreateMessageAsync(message);
                return RedirectToAction("Inbox");
            }

            return View(message);
        }


        [Authorize]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest();
            }

            var message = await _messageStore.GetMessageByIdAsync(id);
            
            if (message == null)
            {
                return NotFound();
            }

            
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            bool isSentMessage = message.SenderID == currentUserId;
            
            // Forbid if user is not the sender or the receiver of message
            if (!isSentMessage)
            {
                if (message.RecipientID != currentUserId)
                {
                    return Forbid();
                }

                // Only updating read status for if user goes on their received messages (not the ones they sent)
                if (!message.Status)
                {
                    message.Status = true;
                    await _messageStore.UpdateMessageAsync(message);
                }
            }

            ViewData["IsSentMessage"] = isSentMessage;
            return View(message);
        }

    }


}