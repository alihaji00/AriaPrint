using AriaPrint.Data_Access_Layer;
using AriaPrint.Repository;

namespace AriaPrint.Models
{
    public class CommentsViewModel
    {
        public Comment NewComment { get; set; } = new Comment();
        public List<Comment> CommentsList { get; set; } = new List<Comment>();
    }
}
