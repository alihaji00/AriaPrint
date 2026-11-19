using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AriaPrint.Controllers
{
    // TODO: ذخیره/ویرایش خبر هنوز پیاده‌سازی نشده.
    // وقتی نوشتی: Content رو سمت سرور sanitize کن (HtmlSanitizer).

    [Authorize(Roles = "Admin,Moderator")]
    public class AdminPanelController : Controller
    {
        public IActionResult Dashboard() => View();
    }
}
