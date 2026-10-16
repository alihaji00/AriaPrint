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
        // header
        public string Title { get; set; }
        public string Category { get; set; }
        public string Author { get; set; } = "کافینت آریا";
        public string ReleaseDate { get; set; }
        // right bar
        public List<string> Documents { get; set; }
        public List<string> Conditions { get; set; }
        public string deadline { get; set; }

    }
}
