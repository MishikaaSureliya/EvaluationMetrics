using Microsoft.AspNetCore.Mvc;

namespace EvolutionMetrics.Controllers
{
    public class DashboardController : Controller
    {
        /// <summary>
        /// Returns the Dashboard index page view.
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }
    }
}
