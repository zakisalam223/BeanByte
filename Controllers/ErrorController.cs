// Controllers/ErrorController.cs
using Microsoft.AspNetCore.Mvc;

namespace forum_aspcore.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/401")]
        public IActionResult UnauthorizedPage()
        {
            return View("UnauthorizedPage");
        }

        [Route("Error/{statusCode}")]
        public IActionResult StatusCodePage(int statusCode)
        {
            switch (statusCode)
            {
                case 404:
                    return View("NotFound");
                case 500:
                    return View("ServerError");
                default:
                    return View("GenericError");
            }
        }
    }
}
