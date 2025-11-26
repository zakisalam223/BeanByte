using forum_aspcore.Models;
using forum_aspcore.Stores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace forum_aspcore.Controllers
{
    public class TagController : Controller
    {
        private readonly MongoTagStore _tagStore;

        public TagController(MongoTagStore tagStore)
        {
            _tagStore = tagStore;
        }

        public async Task<IActionResult> Index()
        {
            var tags = await _tagStore.GetAllTagsAsync();
            return View(tags);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest();

            var tag = await _tagStore.GetTagByIdAsync(id);
            if (tag == null)
            {
                return NotFound();
            }

            return View(tag);
        }

        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(FTag tag)
        {
            ModelState.Remove(nameof(FTag.TagID));
            ModelState.Remove(nameof(FTag.CreatedAt));

            if (ModelState.IsValid)
            {
                // Check if tag with same name already exists
                var existingTag = await _tagStore.GetTagByNameAsync(tag.Name);
                if (existingTag != null)
                {
                    ModelState.AddModelError("Name", "A tag with this name already exists.");
                    return View(tag);
                }

                tag.CreatedAt = DateTime.UtcNow;
                await _tagStore.CreateTagAsync(tag);
                return RedirectToAction(nameof(Index));
            }
            return View(tag);
        }
    }
}
