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
    public class CommentsController : Controller
    {
        private readonly SetComments _commentService = new SetComments();

        [HttpGet]
        public async Task<IActionResult> Comments(int newsId)
        {
            var viewModel = new CommentsViewModel
            {
                CommentsList = await _commentService.GetCommentsAsync(newsId)
            };
            return View(viewModel);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Comments(Comment newComment)
        {
            if (ModelState.IsValid)
            {
                string ip = Request.GetClientIpAddress();
                await _commentService.AddCommentAsync(newComment, ip);
            }
            return RedirectToAction("News", "News", new { id = newComment.NewsId });
        }
    }
}