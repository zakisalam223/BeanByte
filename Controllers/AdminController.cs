using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using forum_aspcore.Models;
using forum_aspcore.Stores;
using System;
using System.Threading.Tasks;
using System.Linq;
using MongoDB.Bson;

namespace forum_aspcore.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly MongoUserStore _userStore;
        private readonly MongoInfractionStore _infractionStore;

        public AdminController(MongoUserStore userStore, MongoInfractionStore infractionStore)
        {
            _userStore = userStore;
            _infractionStore = infractionStore;
        }

        // GET: Admin/IssueInfraction
        public IActionResult IssueInfraction()
        {
            return View(new FInfraction
            {
                DateGiven = DateTime.UtcNow
            });
        }

        // POST: Admin/IssueInfraction
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IssueInfraction(FInfraction model)
        {

            ModelState.Remove(nameof(FInfraction.DateGiven));
            ModelState.Remove(nameof(FInfraction.InfractionID));
            ModelState.Remove(nameof(FInfraction.Reason));
            ModelState.Remove(nameof(FInfraction.InfPointsGiven));
            ModelState.Remove(nameof(FInfraction.GivenByUserID));



            if (ModelState.IsValid)
            {
                var user = await _userStore.GetUserByIdAsync(model.GivenToUserID);

                if (user == null)
                {
                    ModelState.AddModelError("GivenToUserID", "User not found.");
                    return View(model);
                }

                var adminUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value;
                model.GivenByUserID = adminUserId;

                if (string.IsNullOrEmpty(model.InfractionID))
                {
                    model.InfractionID = ObjectId.GenerateNewId().ToString();
                }

                if (model.DateGiven == default)
                {
                    model.DateGiven = DateTime.UtcNow;
                }

                await _infractionStore.CreateInfractionAsync(model);

                user.InfractionPoints = (user.InfractionPoints ?? 0) + model.InfPointsGiven;

                // Check if user should be banned
                if (user.InfractionPoints >= 20 && !user.IsBanned)
                {
                    user.IsBanned = true;
                }

                await _userStore.UpdateUserAsync(user);

                TempData["SuccessMessage"] = "Infraction issued successfully.";
                return RedirectToAction(nameof(IssueInfraction));
            }

            return View(model);
        }
    }
}
