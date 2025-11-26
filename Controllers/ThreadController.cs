// Controllers/ThreadsController.cs
using forum_aspcore.Models;
using forum_aspcore.Stores;
using forum_aspcore.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace forum_aspcore.Controllers
{
    public class ThreadsController : Controller
    {
        private readonly MongoThreadStore _threadStore;
        private readonly MongoUserStore _userStore;
        private readonly MongoRepLogStore _repLogStore;
        private readonly MongoTagStore _tagStore;
        private readonly MongoFileStore _fileStore;
        private readonly MongoSectionStore _sectionStore;
        public ThreadsController(MongoThreadStore threadStore, MongoUserStore userStore, MongoRepLogStore repLogStore, MongoTagStore tagStore, MongoFileStore fileStore, MongoSectionStore sectionStore)
        {
            _threadStore = threadStore;
            _userStore = userStore;
            _repLogStore = repLogStore;
            _tagStore = tagStore;
            _fileStore = fileStore;
            _sectionStore = sectionStore;
        }

        // GET: Threads
        public async Task<IActionResult> Index()
        {
            var threads = await _threadStore.GetAllThreadsAsync();

            var userIds = threads
                .Select(t => t.UserID)
                .Union(threads
                    .Where(t => t.Replies != null) 
                    .SelectMany(t => t.Replies.Select(r => r.UserID))) 
                .Distinct();

            var users = await _userStore.GetUsersByIdsAsync(userIds);
            var userDictionary = users.ToDictionary(u => u.UserID);
            
            // tag info
            var tags = await _tagStore.GetAllTagsAsync();
            var tagDictionary = tags.ToDictionary(t => t.TagID);

            ViewData["UserDictionary"] = userDictionary;
            ViewData["TagDictionary"] = tagDictionary;

            return View(threads);
        }



        // GET: Threads/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest();

            var thread = await _threadStore.GetThreadByIdAsync(id);
            if (thread == null)
            {
                return NotFound();
            }

            // Parse and format the thread description
            thread.Description = EmbedParser.ParseContent(thread.Description, _fileStore);

            // Ensure replies are also included in the thread object
            thread.Replies = thread.Replies?.Select(reply =>
            {
                reply.Content = EmbedParser.ParseContent(reply.Content, _fileStore);
                return reply;
            }).ToList();

            var userIds = new List<string> { thread.UserID }; 
            if (thread.Replies != null)
            {
                userIds.AddRange(thread.Replies.Select(r => r.UserID.ToString()));
            }
            var users = await _userStore.GetUsersByIdsAsync(userIds.Distinct());

            var userDictionary = users.ToDictionary(u => u.UserID, u => u);
            ViewData["UserDictionary"] = userDictionary;

            // Add tag information
            if (thread.TagIDs != null && thread.TagIDs.Any())
            {
                var tags = await _tagStore.GetAllTagsAsync();
                var tagDictionary = tags.ToDictionary(t => t.TagID, t => t);
                ViewData["TagDictionary"] = tagDictionary;
            }
            else
            {
                ViewData["TagDictionary"] = new Dictionary<string, FTag>();
            }

            return View(thread);
        }



        // GET: Threads/Create
        public async Task<IActionResult> Create(string sectionId)
        {
            ModelState.Clear();
            
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Users");
            }

            var sections = await _sectionStore.GetAllSectionsAsync();
            ViewBag.Section = sections;
            
            if (!string.IsNullOrEmpty(sectionId))
            {
                ViewBag.SelectedSectionId = sectionId;
            }

            return View(new FThread());
        }

        // POST: Threads/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FThread thread, string? TagInput)
        {
            ModelState.Remove(nameof(FThread.UserID));
            ModelState.Remove(nameof(FThread.Replies));
            ModelState.Remove(nameof(FThread.SectionID));
            ModelState.Remove(nameof(FThread.CreatedAt));
            ModelState.Remove(nameof(FThread.ThreadID));

            if (string.IsNullOrEmpty(thread.SectionID))
            {
                ModelState.AddModelError("SectionID", "Please select a section");
            }

            if (ModelState.IsValid)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized();
                }

                thread.UserID = userIdClaim.Value;
                thread.CreatedAt = DateTime.UtcNow;
                thread.TagIDs = new List<string>();

                // Process tags
                if (!string.IsNullOrWhiteSpace(TagInput))
                {
                    var tagNames = TagInput.Split(',')
                                         .Select(t => t.Trim())
                                         .Where(t => !string.IsNullOrWhiteSpace(t))
                                         .Distinct();

                    foreach (var tagName in tagNames)
                    {
                        // Check if tag exists
                        var existingTag = await _tagStore.GetTagByNameAsync(tagName);
                        if (existingTag == null)
                        {
                            // Create new tag
                            var newTag = new FTag
                            {
                                Name = tagName,
                                CreatedAt = DateTime.UtcNow
                            };
                            await _tagStore.CreateTagAsync(newTag);
                            thread.TagIDs.Add(newTag.TagID);
                        }
                        else
                        {
                            thread.TagIDs.Add(existingTag.TagID);
                        }
                    }
                }

                await _threadStore.CreateThreadAsync(thread);
                return RedirectToAction("Index", "Section");
            }

            var sections = await _sectionStore.GetAllSectionsAsync();
            ViewBag.Section = sections;
            return View(thread);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostReply(string ThreadID, string Content)
        {
            if (string.IsNullOrEmpty(ThreadID) || string.IsNullOrWhiteSpace(Content))
            {
                TempData["Error"] = "Thread ID and content are required.";
                return RedirectToAction("Details", new { id = ThreadID });
            }

            // Retrieve the thread
            var thread = await _threadStore.GetThreadByIdAsync(ThreadID);
            if (thread == null)
            {
                return NotFound();
            }

            // Create a new reply
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var reply = new FReply
            {
                Content = Content,
                UserID = userIdClaim.Value,
                DatePosted = DateTime.UtcNow,
                ThreadID = ThreadID
            };

            thread.Replies = thread.Replies ?? new List<FReply>();
            thread.Replies.Add(reply);

            var user = await _userStore.GetUserByIdAsync(userIdClaim.Value);
            if (user != null)
            {
                //TODO: Remove this and instead add + or - rep options
                user.RepPoints = (user.RepPoints ?? 0) + 1; // +1 rep point per reply
                await _userStore.UpdateUserAsync(userIdClaim.Value, user);
            }

            await _threadStore.UpdateThreadAsync(ThreadID, thread);

            TempData["Success"] = "Your reply has been posted successfully.";
            return RedirectToAction("Details", new { id = ThreadID });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeReputation(string UserID, string ThreadID, string change)
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(uid))
            {
                return RedirectToAction("Login", "Users");
            }

            if (string.IsNullOrEmpty(UserID) || string.IsNullOrEmpty(ThreadID) || string.IsNullOrEmpty(change))
            {
                TempData["Error"] = "Invalid request.";
                return RedirectToAction("Details", new { id = ThreadID });
            }

            var user = await _userStore.GetUserByIdAsync(UserID);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Details", new { id = ThreadID });
            }

            
            var LocalUser = await _userStore.GetUserByIdAsync(uid);

            if (LocalUser == null)
            {
                TempData["Error"] = "Current user not found.";
                return RedirectToAction("Details", new { id = ThreadID });
            }

            // Check if user is trying to change their own reputation
            if (uid == UserID)
            {
                TempData["Error"] = "You cannot change your own reputation.";
                return RedirectToAction("Details", new { id = ThreadID });
            }

            // Check for cooldown period
            var existingLog = await _repLogStore.GetLogAsync(uid, UserID);
            var cooldownPeriod = TimeSpan.FromDays(5);

            if (existingLog != null && (DateTime.Now - existingLog.LastChangeDate) < cooldownPeriod)
            {
                TempData["Error"] = "Spread some reputation around in the community before changing this users rep again";
                return RedirectToAction("Details", new { id = ThreadID });
            }

            if (LocalUser.RepPower == null)
            {
                TempData["Error"] = "Your reputation power is not set.";
                return RedirectToAction("Details", new { id = ThreadID });
            }

            if (change == "upvote")
            {
                user.RepPoints = (user.RepPoints ?? 0) + LocalUser.RepPower;

                //increase the rep power only if the current user has positive rep power. 
                if (LocalUser.RepPower > 0) user.RepPower++;
                TempData["Success"] = "Reputation increased successfully.";
            }
            else if (change == "downvote")
            {
                user.RepPoints = (user.RepPoints ?? 0) - LocalUser.RepPower;
                if (LocalUser.RepPower > 0) user.RepPower--;
                TempData["Warning"] = "Reputation decreased successfully.";
            }
            else
            {
                TempData["Error"] = "Invalid reputation change type.";
                return RedirectToAction("Details", new { id = ThreadID });
            }

            await _userStore.UpdateUserAsync(UserID, user);

            if (existingLog != null)
            {
                existingLog.LastChangeDate = DateTime.Now;
                await _repLogStore.AddOrUpdateLogAsync(existingLog);
            }
            else
            {
                var newLog = new FReputationLog
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    InitiatorUserID = uid,
                    TargetUserID = UserID,
                    LastChangeDate = DateTime.Now
                };
                await _repLogStore.AddOrUpdateLogAsync(newLog);
            }

            return RedirectToAction("Details", new { id = ThreadID });
        }


        // GET: Threads/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest();

            var thread = await _threadStore.GetThreadByIdAsync(id);
            if (thread == null)
            {
                return NotFound();
            }
            return View(thread);
        }

        // POST: Threads/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, FThread thread)
        {
            if (id != thread.ThreadID)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                await _threadStore.UpdateThreadAsync(id, thread);
                return RedirectToAction(nameof(Index));
            }
            return View(thread);
        }

        // GET: Threads/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest();

            var thread = await _threadStore.GetThreadByIdAsync(id);
            if (thread == null)
            {
                return NotFound();
            }
            return View(thread);
        }

        // POST: Threads/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await _threadStore.DeleteThreadAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
