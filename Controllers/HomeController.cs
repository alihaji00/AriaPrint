using AriaPrint.BL;
using AriaPrint.Data_Access_Layer;
using AriaPrint.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AriaPrint.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SetComments _commentService = new SetComments();

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

        [Route("News/{id}")]
        public async Task<IActionResult> News(int id)
        {
            var NewsPage = new FindNewsPageById(id);
            var News = await NewsPage.GetNews();
            var Model = new NewsViewModel(News);
            return View(Model);
        }


        [HttpGet]
        public async Task<IActionResult> Comments()
        {
            var viewModel = new CommentsViewModel
            {
                CommentsList = await _commentService.GetCommentsAsync()
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Comments(CommentsViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                string? ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                await _commentService.AddCommentAsync(viewModel.NewComment, ip);
            }

            viewModel.CommentsList = await _commentService.GetCommentsAsync();
            return View(viewModel);
        }


        [Route("AllNews")]
        public async Task<IActionResult> AllNews()
        {
            SetAllNewsPage SetAllNewsPage = new();
            var NewsList = await SetAllNewsPage.GetNewsList();
            var Model = new AllNewsViewModel
            {
                News = NewsList
            };
            return View(Model);
        }
    }
}
