using AriaPrint.Repository;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace AriaPrint.Data_Access_Layer
{
    public class SetComments
    {
        ClassSQL db = new();
        public async Task<List<Comment>> GetCommentsAsync(int newsId)
        {
            var Result = await db.SelectAsync<Comment>(@"
            SELECT TOP (1000) [CommentId]
            ,[FullName]
            ,[PhoneNumber]
            ,[Text]
            ,[NewsId]
            ,[IP]
            ,[IsDelete]
            ,[DeleteTime]
            ,[Admin]
            FROM [AriaPrintDB].[dbo].[CommentsTbl]
            WHERE [NewsId] = @NewsId AND [IsDelete] = 0",
            new { NewsId = newsId });

            return Result.ToList();
        }
        public async Task<int> AddCommentAsync(Comment comment, string? ip)
        {
            var insertData = new
            {
                comment.FullName,
                comment.PhoneNumber,
                comment.Text,
                comment.NewsId,
                IP = ip,
                Admin = (string?)null
            };

            return await db.InsertDynamicAsync("CommentsTbl", insertData);
        }
        public async Task<Comment> GetCommentByIdAsync(int id)
        {
            var Result = await db.SelectAsync<Comment>(@"
            SELECT TOP (1) [CommentId]
            ,[FullName]
            ,[PhoneNumber]
            ,[Text]
            ,[NewsId]
            ,[IP]
            ,[IsDelete]
            ,[DeleteTime]
            ,[Admin]
            FROM [AriaPrintDB].[dbo].[CommentsTbl]
            WHERE [CommentId] = @CommentId AND [IsDelete] = 0",
            new { CommentId = id });

            return Result.FirstOrDefault();
        }
    }
}
