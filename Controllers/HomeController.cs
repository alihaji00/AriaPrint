using AriaPrint.BL;
using AriaPrint.Data_Access_Layer;
using AriaPrint.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AriaPrint.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            SetIndexContent SetModelViewModel = new();
            var Model = SetModelViewModel.GetAllLists();
            return View(Model);
        }

        [Route("News/{id}")]
        public IActionResult News(int id)
        {
            var NewsPage = new FindNewsPageById(id);
            var News = NewsPage.GetNews();
            var Model = new NewsViewModel(News);
            return View(Model);
        }
    }
}
