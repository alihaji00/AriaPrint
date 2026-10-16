using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AriaPrint.Controllers
{
    [Authorize(Roles = "Admin,Moderator")]
    public class AdminPanelController : Controller
    {
        public IActionResult Dashboard() => View();
    }
}
