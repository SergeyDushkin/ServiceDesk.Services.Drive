using Microsoft.AspNetCore.Mvc;

namespace servicedesk.Services.Drive.Controllers
{
    [Route("[controller]")]
    public class HomeController : Controller
    {
        public IActionResult Get()
        {
            return Ok(new { name = "servicedesk.Services.Drive" });
        }
    }
}