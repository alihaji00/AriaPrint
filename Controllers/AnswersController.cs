using AriaPrint.BL;
using AriaPrint.Data_Access_Layer;
using AriaPrint.Models;
using AriaPrint.Repository;
using Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AriaPrint.Controllers
{
    public class AnswersController : Controller
    {
        private readonly SetComments _commentService = new SetComments();
        private readonly SetAnswers _answerService = new SetAnswers();

        [HttpGet]
        public async Task<IActionResult> Answers(int CommentId)
        {
            var viewModel = new CommentsViewModel
            {
                AnswersList = await _answerService.GetAnswersAsync(CommentId)
            };
            return View(viewModel);
        }

        [Authorize(Roles = "Admin,Moderator")]
        [HttpPost]
        public async Task<IActionResult> Answers(Answer newAnswer)
        {
            if (ModelState.IsValid)
            {
                string ip = Request.GetClientIpAddress();
                await _answerService.AddAnswerAsync(newAnswer, ip);
            }

            var parentComment = await _commentService.GetCommentByIdAsync(newAnswer.AnswerId);
            if (parentComment == null)
                return RedirectToAction("Index", "Home");

            return RedirectToAction("News", "News", new { id = parentComment.NewsId });
        }
    }
}