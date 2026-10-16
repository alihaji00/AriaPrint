using AriaPrint.Data_Access_Layer;
using AriaPrint.Repository;

namespace AriaPrint.Models
{
    public class NewsViewModel
    {
        public News NewsPage { get; set; }
        public NewsViewModel(News news)
        {
            this.NewsPage=news;
        }
        public List<News> GetNews()
        {
            SetNews setNews = new();
            var NewsList = setNews.GetNews();
            return NewsList;
        }
    }
}