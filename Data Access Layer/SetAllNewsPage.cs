using AriaPrint.Repository;

namespace AriaPrint.Data_Access_Layer
{
    public class SetAllNewsPage
    {
        public async Task<List<News>> GetNewsList()
        {
            var News = new SetNews();
            var NewsList = await News.GetNewsAsync();
            return NewsList;
        }
    }
}