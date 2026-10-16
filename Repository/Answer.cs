namespace AriaPrint.Repository
{
    public class Answer
    {
        public int AnswerId { get; set; }
        public string Text { get; set; }
        public long Date { get; set; }
        public int LikesCount { get; set; }
        public int IsDelete { get; set; }
        public long? DeleteTime { get; set; }
        public string? IP { get; set; }
        public int? Admin { get; set; }
    }
}