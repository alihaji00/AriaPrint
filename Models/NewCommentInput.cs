using System.ComponentModel.DataAnnotations;

namespace AriaPrint.Models
{
    public class NewCommentInput
    {
        public int NewsId { get; set; }

        [Required, StringLength(1000)]
        public string Text { get; set; } = "";
    }
}
