namespace AriaPrint.Repository
{
    public class Feedback
    {
        public string Text { get; set; }
        public string Name { get; set; }
        public string Role { get; set; } = "مشتری";
        public int Rating { get; set; }
    }
}
