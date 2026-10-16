using AriaPrint.Controllers;
using AriaPrint.Data_Access_Layer;
using AriaPrint.Repository;
using System;
using System.Runtime.Intrinsics.X86;

namespace AriaPrint.Models
{
    public class SetIndexContent
    {
        public IndexViewModel GetAllLists()
        {
            var viewModel = new IndexViewModel
            {
                News = GetNewsList(),
                Services = GetServicesList(),
                WhyUs = GetWhyUsList(),
            };
            return viewModel;
        }

        private List<News> GetNewsList()
        {
            var News = new SetNews();
            var NewsList = News.GetNews();
            return NewsList;
        }

        private List<ServicesCard> GetServicesList()
        {
            return new List<ServicesCard>
            {
                new ServicesCard { Title = "پرینت و کپی",       Paragraph = "پرینت سیاه سفید با بهترین کیفیت" },
                new ServicesCard { Title = "ثبت نام",           Paragraph = "ثبت نام انواع طرح ها و سایت های دولتی" },
                new ServicesCard { Title = "خدمات دانشجویی",    Paragraph = "تاییدیه، انتخاب واحد و امور آموزشی" },
                new ServicesCard { Title = "پرداخت بیمه",       Paragraph = "پرداخت بیمه تامین اجتماعی، ورزشی، و سایر" },
                new ServicesCard { Title = "خدمات وسیله نقلیه", Paragraph = "تعویض پلاک، خلافی، کارت سوخت و غیره" },
                new ServicesCard { Title = "خدمات دیگر",        Paragraph = "سایر خدمات اینترنتی و اداری" },

            };
        }

        private List<WhyUsCard> GetWhyUsList()
        {
            return new List<WhyUsCard>
            {
                new WhyUsCard {Title="دقت و اطمینان", Text="اطلاعات شما با دقت بالا ثبت و پردازش می‌شود"},
                new WhyUsCard {Title="سرعت بالا", Text="انجام خدمات در سریع‌ترین زمان ممکن با بهترین کیفیت"},
                new WhyUsCard {Title="تجربه و تخصص", Text="سال‌ها تجربه در ارائه خدمات اینترنتی و اداری"},
                new WhyUsCard {Title="پشتیبانی عالی", Text="پاسخگویی سریع و پشتیبانی همیشگی"},
            };
        }
    }
}
