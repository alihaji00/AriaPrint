using AriaPrint.Controllers;
using AriaPrint.Data_Access_Layer;
using AriaPrint.Repository;
using System;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;

namespace AriaPrint.Models
{
    public class SetIndexContent
    {
        public async Task<IndexViewModel> GetAllLists()
        {
            var viewModel = new IndexViewModel
            {
                News = await GetNewsListAsync(),
                Services = GetServicesList(),
                WhyUs = GetWhyUsList(),
            };
            return viewModel;
        }

        private async Task<List<News>> GetNewsListAsync()
        {
            var News = new SetNews();
            var NewsList = await News.GetNewsAsync();
            return NewsList;
        }

        private List<ServicesCard> GetServicesList()
        {
            return new List<ServicesCard>
            {
                new ServicesCard { Title = "پرینت و کپی",       Paragraph = "پرینت سیاه سفید با بهترین کیفیت",          PictureUrl = "~/images/printer.webp" },
                new ServicesCard { Title = "ثبت نام",           Paragraph = "ثبت نام انواع طرح ها و سایت های دولتی",    PictureUrl="~/images/write.webp" },
                new ServicesCard { Title = "خدمات دانشجویی",    Paragraph = "تاییدیه، انتخاب واحد و امور آموزشی",       PictureUrl="~/images/student.webp" },
                new ServicesCard { Title = "پرداخت بیمه",       Paragraph = "پرداخت بیمه تامین اجتماعی، ورزشی، و سایر", PictureUrl="~/images/trust.webp" },
                new ServicesCard { Title = "خدمات وسیله نقلیه", Paragraph = "تعویض پلاک، خلافی، کارت سوخت و غیره",        PictureUrl="~/images/idCard.webp" },
                new ServicesCard { Title = "خدمات دیگر",        Paragraph = "سایر خدمات اینترنتی و اداری",              PictureUrl="~/images/form.webp" },

            };
        }

        private List<WhyUsCard> GetWhyUsList()
        {
            return new List<WhyUsCard>
            {
                new WhyUsCard {Title="دقت و اطمینان", Text="اطلاعات شما با دقت بالا ثبت و پردازش می‌شود", PictureUrl="~/images/trust.webp" },
                new WhyUsCard {Title="سرعت بالا", Text="انجام خدمات در سریع‌ترین زمان ممکن با بهترین کیفیت", PictureUrl="~/images/trust.webp" },
                new WhyUsCard {Title="تجربه و تخصص", Text="سال‌ها تجربه در ارائه خدمات اینترنتی و اداری", PictureUrl="~/images/trust.webp" },
                new WhyUsCard {Title="پشتیبانی عالی", Text="پاسخگویی سریع و پشتیبانی همیشگی", PictureUrl="~/images/trust.webp" },
            };
        }
    }
}
