using forum_aspcore.Models;
using forum_aspcore.Stores;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Bson;

namespace forum_aspcore.Controllers
{
    public class SectionController : Controller
    {
        private readonly MongoSectionStore _sectionStore;
        private readonly MongoThreadStore _threadStore;
        private readonly MongoUserStore _userStore;
        private readonly MongoTagStore _tagStore;

        public SectionController(
            MongoSectionStore sectionStore,
            MongoThreadStore threadStore,
            MongoUserStore userStore,
            MongoTagStore tagStore)
        {
            _sectionStore = sectionStore;
            _threadStore = threadStore;
            _userStore = userStore;
            _tagStore = tagStore;
        }


        public async Task<ActionResult> Index(string? searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                var allSections = await _sectionStore.GetAllSectionsAsync();
                return View(allSections);
            }

            ViewData["CurrentSearch"] = searchTerm;
            var sections = await _sectionStore.GetSectionBySearchAsync(searchTerm);
            return View(sections);
        }

        public async Task<IActionResult> Details(string id, string? searchTerm)
        {
            var threads = new List<FThread>();

            if (string.IsNullOrEmpty(id))
                return BadRequest();

            var section = await _sectionStore.GetSectionByIdAsync(id);
            if (section == null)
                return NotFound();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                ViewData["CurrentSearch"] = searchTerm;
            }

            // not searching, shows every thread in the section
            if (string.IsNullOrEmpty(searchTerm))
            {
                threads = (List<FThread>)await _threadStore.GetThreadsBySectionIdAsync(id);
                // searching, shows every thread that contains the search term in the section
            }
            else
            {
                threads = (List<FThread>)await _threadStore.GetThreadsBySearchAsync(searchTerm, id);
            }

            // Get user information for threads
            var userIds = threads
                .Select(t => t.UserID)
                .Union(threads
                    .Where(t => t.Replies != null)
                    .SelectMany(t => t.Replies.Select(r => r.UserID)))
                .Distinct();

            var users = await _userStore.GetUsersByIdsAsync(userIds);
            var userDictionary = users.ToDictionary(u => u.UserID);

            // Get tag information
            var tags = await _tagStore.GetAllTagsAsync();
            var tagDictionary = tags.ToDictionary(t => t.TagID);

            ViewData["Section"] = section;
            ViewData["UserDictionary"] = userDictionary;
            ViewData["TagDictionary"] = tagDictionary;

            return View(threads);

        }

        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(FSection section)
        {
            try
            {
                section.SectionID = ObjectId.GenerateNewId().ToString();

                await _sectionStore.CreateSectionAsync(section);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error creating section: " + ex.Message);
                return View(section);
            }
        }
    }
}
