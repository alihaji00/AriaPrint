using AriaPrint.BL;
using AriaPrint.Data_Access_Layer;
using AriaPrint.Models;
using AriaPrint.Repository;
using Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AriaPrint.Controllers
{
    public class CommentsController : Controller
    {
        private readonly SetComments _commentService = new SetComments();

        //UseLess Action:

        //[HttpGet]
        //public async Task<IActionResult> Comments(int newsId)
        //{
        //    var viewModel = new CommentsViewModel
        //    {
        //        CommentsList = await _commentService.GetCommentsAsync(newsId)
        //    };
        //    return View(viewModel);
        //}

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Comments(NewCommentInput input)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("News", "News", new { id = input.NewsId });
            }

            var comment = new Comment
            {
                NewsId = input.NewsId,
                Text = input.Text,
                FullName = $"{User.FindFirst(ClaimTypes.GivenName)?.Value} {User.FindFirst(ClaimTypes.Surname)?.Value}".Trim(),
                PhoneNumber =long.Parse(User.FindFirst(ClaimTypes.MobilePhone)!.Value),
            };

            string ip = Request.GetClientIpAddress();
            await _commentService.AddCommentAsync(comment, ip);

            return RedirectToAction("News", "News", new { id = input.NewsId });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Delete(int commentId)
        {
            var comment = await _commentService.GetCommentByIdAsync(commentId);
            if (comment.CommentId == 0) return NotFound();

            var isMod = User.IsInRole("Admin") || User.IsInRole("Moderator");
            var myPhone = long.Parse(User.FindFirst(ClaimTypes.MobilePhone)!.Value);

            if (!isMod && comment.PhoneNumber != myPhone)
            {
                return StatusCode(403); //You Cant delete comment from others Unless your an admin
            }

            await _commentService.DeleteCommentAsync(commentId);
            return Ok();
        }

        //public IActionResult ToggleLike(){}
    }
}   