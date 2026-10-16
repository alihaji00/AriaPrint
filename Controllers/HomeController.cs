using AriaPrint.BL;
using AriaPrint.Data_Access_Layer;
using AriaPrint.Models;
using AriaPrint.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Threading.Tasks;
using Common.Extensions;

namespace AriaPrint.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SetComments _commentService = new SetComments();
        private readonly SetAnswers _answerService = new SetAnswers();

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

            ViewData["CommentsModel"] = new CommentsViewModel
            {
                CommentsList = await _commentService.GetCommentsAsync(id),
                NewComment = new Comment { NewsId = id }
            };

            return View(Model);
        }

        [HttpGet]
        public async Task<IActionResult> Comments(int newsId)
        {
            var viewModel = new CommentsViewModel
            {
                CommentsList = await _commentService.GetCommentsAsync(newsId)
            };
            return View(viewModel);
        }

        //[HttpPost]
        //public async Task<IActionResult> Comments(CommentsViewModel viewModel)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        string ip = Request.GetClientIpAddress();
        //        await _commentService.AddCommentAsync(viewModel.NewComment, ip);
        //    }
        //    else
        //    {
        //        foreach (var kvp in ModelState)
        //            foreach (var err in kvp.Value.Errors)
        //                Console.WriteLine($"{kvp.Key}: {err.ErrorMessage}");
        //    }

        //    return RedirectToAction("News", new { id = viewModel.NewComment.NewsId });
        //}

        [HttpPost]
        public async Task<IActionResult> Comments(Comment newComment)
        {
            if (ModelState.IsValid)
            {
                string ip = Request.GetClientIpAddress();
                await _commentService.AddCommentAsync(newComment, ip);
            }
            return RedirectToAction("News", new { id = newComment.NewsId });
        }

        [HttpPost]
        public async Task<IActionResult> Answers(Answer newAnswer)
        {
            if (ModelState.IsValid)
            {
                string ip = Request.GetClientIpAddress();
                await _answerService.AddAnswerAsync(newAnswer, ip);
            }

            var parentComment = await _commentService.GetCommentByIdAsync(newAnswer.AnswerId);
            return RedirectToAction("News", new { id = parentComment.NewsId });
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
