using forum_aspcore.Models;
using forum_aspcore.Stores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using System.Security.Claims;
using System.Threading.Tasks;

namespace forum_aspcore.Controllers
{
    public class FileController : Controller
    {
        private readonly MongoFileStore _fileStore;
        private readonly MongoUserStore _userStore;

        public FileController(MongoFileStore fileStore, MongoUserStore userStore)
        {
            _fileStore = fileStore;
            _userStore = userStore;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Users");
            }

            var files = await _fileStore.GetFilesByUserIdAsync(userId);
            return View(files);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminPendingFiles()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Users");
            }
            var pendingFiles = await _fileStore.GetPendingFilesAsync();
            return View(pendingFiles);
        }

        // Approve File Action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveFile(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _userStore.GetUserByIdAsync(userId);
            if (currentUser == null || string.IsNullOrEmpty(currentUser.AdminID))
            {
                return Unauthorized();
            }

            var result = await _fileStore.ApproveFileAsync(id);
            if (result)
            {
                TempData["Success"] = "File approved successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to approve the file.";
            }

            return RedirectToAction(nameof(AdminPendingFiles));
        }

        // Deny File Action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DenyFile(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _userStore.GetUserByIdAsync(userId);
            if (currentUser == null || string.IsNullOrEmpty(currentUser.AdminID))
            {
                return Unauthorized();
            }

            var result = await _fileStore.DenyFileAsync(id);
            if (result)
            {
                TempData["Success"] = "File denied successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to deny the file.";
            }

            return RedirectToAction(nameof(AdminPendingFiles));
        }

        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please select a file to upload.";
                return RedirectToAction(nameof(Upload));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Users");
            }

            try
            {
                // Create a new FFile
                var fileDoc = new FFile
                {
                    Status = false,
                    UploadDate = DateTime.UtcNow,
                    UploadedBy = userId,
                    Filename = file.FileName
                };

                // Upload file to GridFS and save document
                using (var stream = file.OpenReadStream())
                {
                    var gfsId = await _fileStore.SaveToGridFSAsync(stream, file.FileName, "userfile");
                    fileDoc.GFSID = gfsId;
                    await _fileStore.SaveFileDocumentAsync(fileDoc);
                }

                TempData["Success"] = "File uploaded successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to upload file. Please try again.";
                return RedirectToAction(nameof(Upload));
            }
        }

        public async Task<IActionResult> Download(string id)
        {
            var file = await _fileStore.GetFileByIdAsync(id);
            if (file == null)
            {
                return NotFound();
            }

            // Check if file is verified
            if (!file.Status)
            {
                TempData["Error"] = "Download failed. File not verified.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var fileBytes = await _fileStore.DownloadFileAsync(file.GFSID);
                return File(fileBytes, "application/octet-stream", file.Filename);
            }
            catch (Exception e)
            {

                TempData["Error"] = "Failed to download file.";
                return RedirectToAction(nameof(Index));
            }
        }

         public async Task<IActionResult> DeleteFile(string id)
        {
            var file = await _fileStore.GetFileByIdAsync(id);
            if (file == null)
            {
                return NotFound();
            }
            await _fileStore.DeleteFileAsync(file.FileID, file.GFSID);
            return RedirectToAction(nameof(Index));
        }
    }
}