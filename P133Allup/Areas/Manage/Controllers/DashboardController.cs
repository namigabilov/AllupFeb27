using Microsoft.AspNetCore.Mvc;

namespace P133Allup.Areas.Manage.Controllers
{
    public class DashboardController : Controller
    {
        [Area("manage")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
