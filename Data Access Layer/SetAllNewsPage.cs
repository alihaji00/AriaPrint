using AriaPrint.Repository;

namespace AriaPrint.Data_Access_Layer
{
    public class SetAllNewsPage
    {
        public List<News> GetNewsList()
        {
            var News = new SetNews();
            var NewsList = News.GetNews();
            return NewsList;
        }
    }
}
