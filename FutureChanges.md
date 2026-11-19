## کارهای باقی‌مانده

### قبل از deploy
- [ ] **`AllowedHosts`**: توی `appsettings.json` مقدارش `"*"` است. توی `appsettings.Production.json` دامنه واقعی گذاشته بشه (`"ariaprint.ir;www.ariaprint.ir"`).
- [ ] **`ForwardedHeaders`**: بدون آن، پشت پروکسی همه کاربرها یک IP می‌شوند و rate limit لاگین برای همه قفل می‌شود. با `KnownProxies` تنظیم بشه و قبل از `UseRateLimiter` بیاید.
- [ ] **`GetClientIpAddress`** (`Tools/Extensions.cs`، خط ۱۵۴۳): به هدرهای قابل جعل اعتماد می‌کند. بعد از تنظیم `ForwardedHeaders` با `Connection.RemoteIpAddress` جایگزین بشه.
- [ ] **Content-Security-Policy**: اول اسکریپت‌ها و استایل‌های inline به فایل جدا منتقل بشن، بعد CSP اضافه بشه.

### امنیت
- [ ] **sanitize سمت سرور برای محتوای خبر** (قبل از نوشتن بک‌اند پنل ادمین): با `HtmlSanitizer` روی `Content` قبل از ذخیره. تا آن موقع `Html.Raw` استفاده نشود.
- [ ] **بررسی نقش کاربر از دیتابیس** (`OnValidatePrincipal` در `Program.cs`): وقتی مدیریت کاربران نوشته شد.
- [ ] **Lockout روی حساب کاربری** (اختیاری): ستون‌های `FailedLoginCount` و `LockoutEnd` توی `UsersTbl`.

### قابلیت‌ها
- [ ] **«مرا به خاطر بسپار»**: `LoginViewModel.RememberMe`، `SignInUserAsync(user, rememberMe)`، و `ExpireTimeSpan` کوتاه‌تر برای حالت بدون تیک (`Login.cshtml` خط ۱۰۷، `AccountController.cs` خط ۱۲۷).
- [ ] **فیلتر و جستجو توی «همه اخبار»**: دسته (پیام نور، آزاد، ...) و جستجوی متنی (مثلاً «استخدامی بانک سینا»). توی SQL و پارامتریک نوشته بشه، نه توی حافظه. (`AllNews.cshtml`، `NewsController.AllNews()`، `SetAllNewsPage`)