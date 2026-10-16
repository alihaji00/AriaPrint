using AriaPrint.Data_Access_Layer;
using AriaPrint.Repository;

namespace AriaPrint.BL
{
    public class FindNewsPageById
    {
        public int PageId { get; set; }

        public FindNewsPageById(int Id)
        {
            this.PageId = Id;
        }

        public News GetNews()
        {
            SetNews SetNews = new();
            var NewsList = SetNews.GetNews();
            var News = NewsList.FirstOrDefault(n => n.Id == PageId);
            return (News);
        }
    }
}