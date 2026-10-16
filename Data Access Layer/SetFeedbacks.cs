using AriaPrint.Repository;

namespace AriaPrint.Models
{
    public class SetFeedbacks
    {
        public List<Feedback> GetFeedbacks()
        {
            var Feedbacks = new List<Feedback>
            {
                new Feedback
                {
                    Text = "از سرعت عمل و برخورد حرفه‌ای کافینت آریا بسیار راضی هستم. همیشه بهترین انتخاب من بوده‌اند",
                    Name = "مریم احمدی",
                    Role = "دانشجو",
                    Rating = 5,
                },
                new Feedback
                {
                    Text = "یکی از بهترین مساجر هایی که طی صد سال گذشته داشتم. به خوبی میتونم روز اولی که دیدمشون رو به یاد بیارم",
                    Name = "مستر کریم پور",
                    Role = "مالک مهربان",
                    Rating = 5,
                },
            };
            return Feedbacks;
        }
    }
}
