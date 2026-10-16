using AriaPrint.Data_Access_Layer;
using AriaPrint.Repository;

namespace AriaPrint.Models
{
    public class NewsViewModel
    {
        public News NewsPage { get; set; }
        public NewsViewModel(News news)
        {
            this.NewsPage = news;
        }
        public async Task<List<News>> GetNewsAsync()
        {
            SetNews setNews = new();
            var NewsList = await setNews.GetNewsAsync();
            return NewsList;
        }
    }
}