using Microsoft.AspNetCore.Mvc;

namespace AriaPrint.Controllers
{
    public class RequestController : Controller
    {
        [Route("/SubmitRequest")]
        public IActionResult SubmitRequest()
        {
            return View();
        }
    }
}
