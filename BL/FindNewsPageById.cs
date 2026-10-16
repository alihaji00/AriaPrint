using AriaPrint.Data_Access_Layer;
using AriaPrint.Repository;
using System.Threading.Tasks;

namespace AriaPrint.BL
{
    public class FindNewsPageById
    {
        public int PageId { get; set; }

        public FindNewsPageById(int Id)
        {
            this.PageId = Id;
        }

        public async Task<News> GetNews()
        {
            SetNews SetNews = new();
            var NewsList = await SetNews.GetNewsAsync();
            var News = NewsList.FirstOrDefault(n => n.Id == PageId);
            return (News);
        }
    }
}