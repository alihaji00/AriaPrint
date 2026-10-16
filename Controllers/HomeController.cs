using AriaPrint.BL;
using AriaPrint.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace AriaPrint.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            SetIndexContent SetModelViewModel = new();
            var Model = await SetModelViewModel.GetAllLists();
            return View(Model);
        }
    }
}