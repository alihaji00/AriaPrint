using System.Text.Json;

namespace AriaPrint.Repository
{
    public class News
    {
        public int Id { get; set; }
        public string Content { get; set; }
        //IndexCard
        public bool IsNew { get; set; }
        public string PictureUrl { get; set; } = "#ph-mono";
        public string Summary { get; set; }
        public string PageUrl { get; set; } = "#";
        // news page landing
        public string Title { get; set; }
        public string Category { get; set; }
        public string Author { get; set; } = "کافینت آریا";
        public string ReleaseDate{ get; set; }
        // news page right bar
        public string deadline { get; set; }
        public string DocumentsJson { get; set; }
        public string ConditionsJson { get; set; }

        // این دوتا فقط برای استفاده‌ی راحت توی کد و View هستن
        public List<string> Documents =>
            string.IsNullOrWhiteSpace(DocumentsJson)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(DocumentsJson);

        public List<string> Conditions =>
            string.IsNullOrWhiteSpace(ConditionsJson)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(ConditionsJson);

        public string ReleaseDateFormatted
        {
            get
            {
                if (string.IsNullOrWhiteSpace(ReleaseDate) || ReleaseDate.Length != 8)
                    return ReleaseDate;

                var year = ReleaseDate.Substring(0, 4);
                var month = ReleaseDate.Substring(4, 2);
                var day = ReleaseDate.Substring(6, 2);

                return $"{year}/{month}/{day}";
            }
        }

        public string DeadlineFormatted
        {
            get
            {
                if (string.IsNullOrWhiteSpace(ReleaseDate) || ReleaseDate.Length != 8)
                    return ReleaseDate;

                var year = ReleaseDate.Substring(0, 4);
                var month = ReleaseDate.Substring(4, 2);
                var day = ReleaseDate.Substring(6, 2);

                return $"{year}/{month}/{day}";
            }
        }

    }
}
