using Microsoft.AspNetCore.Mvc;

namespace WebApplicationTestingCube.Controllers
{
    [Route("home")]
    public class HomeController : Controller
    {
        // This will serve the index.cshtml page when you visit /home
        public IActionResult Index()
        {
            return View(); // Returns the index.cshtml page from the SSAS folder
        }
    }
}
