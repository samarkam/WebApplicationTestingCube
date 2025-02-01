using Microsoft.AspNetCore.Mvc;

namespace WebApplicationTestingCube.Controllers
{
    [Route("home")]
    public class HomeController : Controller
    {
    
        public IActionResult Index()
        {
          

            return View();
        }
    }
}
