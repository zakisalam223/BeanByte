// Controllers/UsersController.cs
using Microsoft.AspNetCore.Mvc;
using forum_aspcore.Models;
using forum_aspcore.Stores;
using System.Threading.Tasks;
using BCrypt.Net; 
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using forum_aspcore.Services;
using MongoDB.Bson;
using MongoDB.Driver.GridFS;

namespace forum_aspcore.Controllers
{
    public class UsersController : Controller
    {
        private readonly MongoUserStore _userStore;
        private readonly MongoFileStore _fileStore;
        private readonly MongoInfractionStore _infractionStore;

        public UsersController(MongoUserStore userStore, MongoFileStore fileStore, MongoInfractionStore infractionStore)
        {
            _userStore = userStore;
            _fileStore = fileStore;
            _infractionStore = infractionStore;
        }

        // GET: Users/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Users/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(FRegister model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _userStore.GetUserByUsernameAsync(model.Username);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Username", "Username already exists.");
                    return View(model);
                }

                existingUser = await _userStore.GetUserByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Email already registered.");
                    return View(model);
                }

                var user = new FUser
                {
                    Username = model.Username,
                    Email = model.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    RepPower = 0,
                    RepPoints = 0,
                    InfractionPoints = 0,
                    IsBanned = false,
                    JoinDate = System.DateTime.Now
                };

                await _userStore.CreateUserAsync(user);
                TempData["SuccessMessage"] = "Registration successful! You can now log in.";
                return RedirectToAction(nameof(Login)); 
            }

            return View(model);
        }

        // GET: Users/Login
        public IActionResult Login()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); 
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Users"); 
            }

            var user = await _userStore.GetUserByIdAsync(userId); 
            if (user == null)
            {
                return NotFound(); 
            }

            return View(user); 
        }

        [HttpGet]
        public async Task<IActionResult> ProfilePicture(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return GetDefaultProfilePicture();
            }

            var user = await _userStore.GetUserByIdAsync(id);
            if (user == null || user.GFSID_PFP == null)
            {
                return GetDefaultProfilePicture();
            }

            try
            {
                var fileBytes = await _fileStore.DownloadFileAsync(user.GFSID_PFP.Value);

                // Retrieve the ContentType from FFile
                var fileDoc = await _fileStore.GetFileByGFSIdAsync(user.GFSID_PFP.Value);
                var contentType = "image/jpeg"; // default
                if (fileDoc != null && !string.IsNullOrEmpty(fileDoc.ContentType))
                {
                    contentType = fileDoc.ContentType;
                }

                return File(fileBytes, contentType);
            }
            catch (Exception ex)
            {
                return GetDefaultProfilePicture();
            }
        }

        private IActionResult GetDefaultProfilePicture()
        {
            try
            {
                var defaultImageUrl = "https://www.refugee-action.org.uk/wp-content/uploads/2016/10/anonymous-user.png";

                using (var httpClient = new HttpClient())
                {
                    var response = httpClient.GetAsync(defaultImageUrl).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var imageBytes = response.Content.ReadAsByteArrayAsync().Result;
                        var contentType = response.Content.Headers.ContentType?.MediaType ?? "image/jpeg";
                        return File(imageBytes, contentType);
                    }
                }
            }
            catch
            {
                return NotFound();
            }

            return NotFound();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(FUser model, IFormFile ProfilePicture)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Users");
            }

            var user = await _userStore.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            user.Bio = model.Bio;
            user.Degree = model.Degree;
            user.YearOfDegree = model.YearOfDegree;
            user.Signature = model.Signature;

            if (!string.IsNullOrEmpty(model.Title))
            {
                var validRoles = new List<string> { "Student", "TA", "Professor" };
                if (!validRoles.Contains(model.Title))
                {
                    ModelState.AddModelError("Title", "Invalid role selection.");
                    return View(model);
                }
                user.Title = model.Title;

                if (model.Title == "Professor")
                {
                    if (string.IsNullOrEmpty(model.AdminID))
                    {
                        ModelState.AddModelError("AdminID", "Professor ID is required.");
                        return View(model);
                    }
                    if (string.IsNullOrEmpty(model.Course))
                    {
                        ModelState.AddModelError("Course", "Course is required for Professors.");
                        return View(model);
                    }
                    user.AdminID = model.AdminID;
                    user.Course = model.Course;
                    user.UCID = null;
                }
                else if (model.Title == "TA")
                {
                    if (string.IsNullOrEmpty(model.Course))
                    {
                        ModelState.AddModelError("Course", "Course is required for TAs.");
                        return View(model);
                    }
                    user.Course = model.Course;
                    user.AdminID = null;
                    user.UCID = null;
                }
                else if (model.Title == "Student")
                {
                    if (string.IsNullOrEmpty(model.UCID))
                    {
                        ModelState.AddModelError("UCID", "UCID is required for Students.");
                        return View(model);
                    }
                    user.UCID = model.UCID;
                    user.Course = null;
                    user.AdminID = null;
                }
            }

            if (ProfilePicture != null && ProfilePicture.Length > 0)
            {
                var fileExtension = Path.GetExtension(ProfilePicture.FileName).ToLower();
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("ProfilePicture", "Invalid image format. Allowed formats: .jpg, .jpeg, .png, .gif");
                    return View(model);
                }

                const long maxFileSize = 2 * 1024 * 1024;
                if (ProfilePicture.Length > maxFileSize)
                {
                    ModelState.AddModelError("ProfilePicture", "Image size exceeds the 2MB limit.");
                    return View(model);
                }

                if (user.GFSID_PFP.HasValue)
                {
                    try
                    {
                        var existingFile = await _fileStore.GetFileByGFSIdAsync(user.GFSID_PFP.Value);
                        if (existingFile != null)
                        {
                            await _fileStore.DeleteFileAsync(existingFile.FileID, existingFile.GFSID);
                        }
                    }
                    catch
                    {
                    }
                }

                try
                {
                    using (var stream = ProfilePicture.OpenReadStream())
                    {
                        var gfsId = await _fileStore.SaveToGridFSAsync(stream, ProfilePicture.FileName, ProfilePicture.ContentType);

                        var fileDoc = new FFile
                        {
                            FileID = ObjectId.GenerateNewId().ToString(),
                            GFSID = gfsId,
                            UploadedBy = userId,
                            ContentType = ProfilePicture.ContentType,
                            Status = true,
                            Filename = ProfilePicture.FileName + " (Profile Picture)",
                            UploadDate = DateTime.Now
                        };

                        await _fileStore.SaveFileDocumentAsync(fileDoc);

                        user.GFSID_PFP = gfsId;
                    }
                }
                catch
                {
                    ModelState.AddModelError("ProfilePicture", "An error occurred while uploading the profile picture.");
                    return View(model);
                }
            }

            try
            {
                await _userStore.UpdateUserAsync(user);
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "An error occurred while updating your profile.");
                return View(model);
            }

            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction("Details");
        }


        public async Task<IActionResult> Details()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); 
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Users"); 
            }

            var infractions = await _infractionStore.GetInfractionsByUserAsync(userId);
            ViewBag.Infractions = infractions;
            ViewBag.IsCurrentUser = true;

            var user = await _userStore.GetUserByIdAsync(userId); 
            if (user == null)
            {
                return NotFound(); 
            }

            return View(user);
        }

        // POST: Users/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(FLogin model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userStore.GetUserByUsernameAsync(model.Username);
                if (user != null && BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                {
                    if (user.InfractionPoints >= 20)
                    {
                        ModelState.AddModelError(string.Empty, "You are banned from the forum.");
                        return View(model);
                    }
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID),
                new Claim(ClaimTypes.Name, user.Username)
            };

                    // Add role claim if the user is an admin
                    if (!string.IsNullOrEmpty(user.AdminID))
                    {
                        claims.Add(new Claim(ClaimTypes.Role, "Admin"));
                    }

                    var claimsIdentity = new ClaimsIdentity(claims, "Login");

                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    await HttpContext.SignInAsync(claimsPrincipal);

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View(model);
        }

        // POST: Users/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(); // Sign out user
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> ViewProfile(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("Index", "Home");
            }

            var user = await _userStore.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var infractions = await _infractionStore.GetInfractionsByUserAsync(id);
            ViewBag.Infractions = infractions;
            
            // flag for checking if this is the current user's profile (if its not, then we won't see the "edit profile" button)
            ViewBag.IsCurrentUser = User.FindFirstValue(ClaimTypes.NameIdentifier) == id;

            return View("Details", user);
        }

    }
}
