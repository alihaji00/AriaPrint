using AriaPrint.BL;
using AriaPrint.Data_Access_Layer;
using AriaPrint.Models;
using AriaPrint.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AriaPrint.Controllers
{
    public class NewsController : Controller
    {
        private readonly SetComments _commentService = new SetComments();
        private readonly SetAnswers _answerService = new SetAnswers();

        [Route("News/{id}")]
        public async Task<IActionResult> News(int id)
        {
            var NewsPage = new FindNewsPageById(id);
            var News = await NewsPage.GetNews();
            var Model = new NewsViewModel(News);

            var comments = await _commentService.GetCommentsAsync(id);
            var answers = new List<Answer>();
            foreach (var c in comments)
            {
                answers.AddRange(await _answerService.GetAnswersAsync(c.CommentId));
            }

            ViewData["CommentsModel"] = new CommentsViewModel
            {
                CommentsList = comments,
                NewComment = new Comment { NewsId = id },
                AnswersList = answers,
                NewAnswer = new Answer { AnswerId = 0, Text = "" }
            };

            return View(Model);
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