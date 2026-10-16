using System.Numerics;

namespace AriaPrint.Repository
{
    public class Comment
    {
        public int CommentId { get; set; }
        public string FullName { get; set; }
        public long PhoneNumber { get; set; }
        public string Text { get; set; }
        public int NewsId { get; set; }
        public int Date { get; set; }
        public int IsDelete { get; set; }
        public long? DeleteTime { get; set; }
        public string? IP { get; set; }
        public int? Admin { get; set; }
    }
}
