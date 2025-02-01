using Microsoft.AspNetCore.Mvc;

namespace WebApplicationTestingCube.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
    
        public IActionResult Index()
        {
          

            return View();
        }
    }
}
